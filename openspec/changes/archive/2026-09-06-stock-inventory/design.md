## Context

See `proposal.md` — Why. Product has no inventory field; cart (`HomeController.Details` POST, `CartController.Plus`) and checkout (`CheckoutService.CreateOrder` inside `ExecuteInTransaction`) never check quantity. Adding a single `StockQuantity` column with validation at the two user entry points and an atomic check-and-decrement at checkout closes the oversell gap without introducing reservations, history, or low-stock thresholds. Existing patterns to reuse: `Repository`/`UnitOfWork`, `HomeController.Details` POST pattern with `TempData["error"]` + localize + redirect, `CheckoutService` transaction boundary, `_localizer` RESX per controller, DataTables `ProductController.GetAll` JSON.

## Goals / Non-Goals

**Goals:**
- Single source of truth for available units per product (`Product.StockQuantity`).
- Admin visibility and editing with server-side validation.
- No oversell: add-to-cart and plus-button guard + checkout atomic validate-and-decrement.
- Stock never goes negative, even under concurrent checkouts.

**Non-Goals:**
- Low-stock alerts, reorder points, stock history/audit, reservations/holds, backorders, multi-warehouse, or variants.
- Decrement at payment-approved time, restock on cancel/return, or shipping-cost-as-line.
- Product availability filtering by stock (out-of-stock products remain visible but not addable — avoids SEO churn).
- New packages or tenant/multi-store branching.

## Decisions

**1. Column type: `int StockQuantity` with `[Range(0,int.MaxValue)]`, default 0.**
- Alternatives: `decimal`, `uint`, nullable. Rejected: `Product.Count` and all order counts are `int`; decimal adds no value for whole units. Nullable would require null→0 coalescing everywhere. Default 0 is the safe choice (no accidental oversell on new products); seeds/migrations can bulk-set real stock.

**2. Validate at the two write edges + atomically at checkout — not at cart read.**
- `HomeController.Details` POST: need `product.StockQuantity` + existing cart count for that user/product; reject if `existing + requested > stock`. Redirect to product detail with `TempData["error"]`.
- `CartController.Plus`: fetch full product, compare `cart.Count + 1 > product.StockQuantity`; reject with `TempData`.
- `CheckoutService.CreateOrder`: re-read each product inside `ExecuteInTransaction` (already the transaction boundary), verify `cart.Count <= product.StockQuantity`, then `product.StockQuantity -= cart.Count` and `Update`. If any line fails, return a `CheckoutCreateOrderResult` with `InsufficientStock = true` (new flag) and the caller (`CartController.SummaryPOST`) redirects to `Index` with error — no order/header/details persisted.
- Alternative considered: DB `CHECK (StockQuantity >= 0)` + catching exception. Keeps it as a safety net but not the primary UX path — user-facing validation stays in code for localized messages. Alternative considered: pessimistic `FOR UPDATE` / row lock — EF Core on Npgsql doesn't expose it cleanly for this repository pattern; the single transaction with read-then-write suffices for low contention (small store) and the `CHECK` constraint guarantees no negative slip-through. Document as `// ponytail: single-transaction decrement, SELECT FOR UPDATE if contention grows`.

**3. Decrement at order creation, for all order kinds.**
- Alternative: decrement only when payment becomes `Approved` (Stripe/MP webhook). Rejected: adds state to reconcile, bank-transfer/Company delayed-payment have different approved moments, and oversell window widens to payment delay. Eager decrement at `CreateOrder` reserves inventory immediately; restock-on-cancel is deferred (no cancel flow today). Simplest that prevents oversell now.

**4. Admin surface: extend `ProductController.Upsert` + `GetAll`.**
- `Upsert` GET: populate; POST: validate `StockQuantity` via DataAnnotations, map through `UnitOfWork.UpdateEntityValues` path already used. `GetAll` JSON: add `StockQuantity` to anonymous projection (or return entity — currently returns `List<Product>` directly, so include it). View: add number input `min=0` next to price/category; DataTables column for stock. No new controller.

**5. Storefront UX: no hard hide, stepper capped.**
- Product `Details.cshtml` shows "Out of stock" badge when `StockQuantity == 0` and disables Add-to-Cart. `details.js` stepper `max` is set from a `data-stock` attribute so Plus button is disabled at limit — mirrors server guard but doesn't replace it.

**6. Concurrency note.**
- Current `ExecuteInTransaction` uses a single `DbContext` transaction. Two concurrent checkouts serialize on the DB transaction; the second re-reads the already-decremented stock and fails validation before writing. No distributed lock. If the store scales, migrate to `SELECT ... FOR UPDATE` or optimistic concurrency with a `RowVersion` on `Product`.

## Risks / Trade-offs

- **Race still possible without row lock under high concurrency** → Mitigation: `CHECK (StockQuantity >= 0)` constraint + transaction re-read; monitor checkout 500s. Upgrade to `FOR UPDATE` when needed.
- **Existing products all start at 0 stock — could look empty after deploy** → Mitigation: migration default 0 is safe; include a one-time data fix (admin bulk-edit or a temporary SQL update in `DbInitializer`) and document it in migration plan.
- **Admin bulk stock entry is tedious (no CSV import)** → Accepted for now; manual per-product edit covers the small catalog.
- **Cart that was valid at add time can become invalid by checkout (stock sold out)** → Mitigation: checkout validation fails with clear message; user returns to cart to adjust.
- **Outdated cart removal (`RemoveShoppingCartsOutdated`) currently only checks `IsDeleted`/`IsAvailableInStore` — not stock** → Keep that behavior; stock mismatch is handled at the operation that matters (add/plus/checkout), not by silently purging cart lines.

## Migration Plan

1. Add `Product.StockQuantity` property (`[Range(0,int.MaxValue)]` + `LocalizedRequired` not needed — default 0).
2. `dotnet ef migrations add AddProductStockQuantity` — generates `ALTER TABLE "Products" ADD COLUMN "StockQuantity" integer NOT NULL DEFAULT 0`.
3. Optional follow-up migration/seed step: `UPDATE "Products" SET "StockQuantity" = <real counts>` or let admin set via Upsert.
4. Deploy `MigrateOnStartup` disabled in production per `core.md` — run migrations via `dotnet ef database update` or the store compose entrypoint.
5. Rollback: `dotnet ef database update <prev>` drops column; code with stock guards will fail to start until reverted — deploy code rollback together.

## Open Questions

- Initial stock values for existing products? (Proposal assumes admin will set them; a follow-up could seed from an external sheet.)
- Should `IsAvailableInStore` be auto-toggled when stock hits 0? Deferred — keeping the flags independent avoids surprising hides.
