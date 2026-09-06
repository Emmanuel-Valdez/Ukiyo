## 1. Model & Migration

- [x] 1.1 Add `StockQuantity` (int, >=0, default 0) to `VaultShop.Models/Product.cs` with `[LocalizedRange(0,int.MaxValue,...)]` and verify `dotnet build` compiles
- [x] 1.2 Create EF Core migration `AddProductStockQuantity` and verify it generates `ALTER TABLE "Products" ADD COLUMN "StockQuantity" integer NOT NULL DEFAULT 0`
- [x] 1.3 Verify `ApplicationDbContextModelSnapshot` reflects `StockQuantity` and `dotnet build --no-restore` is clean

## 2. Admin Stock Management

- [x] 2.1 Expose `StockQuantity` in `ProductController.GetAll` JSON and verify DataTables product list shows the stock column (manual browser check)
- [x] 2.2 Add stock number input (`min=0`) to `Areas/Admin/Views/Product/Upsert.cshtml` with server-side validation, and verify create/edit persists stock and rejects negative values with a validation message
- [x] 2.3 Add EN/ES localization for stock field labels and validation messages and verify both cultures render correctly

## 3. Storefront Add-to-Cart & Cart Guards

- [x] 3.1 Guard `HomeController.Details` POST (add-to-cart): check `existingCartCount + requested > product.StockQuantity`, reject with localized `TempData["error"]` and verify adding beyond stock is blocked with the error message
- [x] 3.2 Guard `CartController.Plus`: reject increment when `cart.Count + 1 > product.StockQuantity` with localized error, keep `Minus`/`Remove` unrestricted, and verify via manual cart flow
- [x] 3.3 Show out-of-stock state on `Areas/Customer/Views/Home/Details.cshtml` (badge + disabled Add-to-Cart when `StockQuantity == 0`) and cap the quantity stepper `max=data-stock`, verify light/dark + mobile

## 4. Checkout Atomic Validation & Decrement

- [x] 4.1 In `CheckoutService.CreateOrder`, inside `ExecuteInTransaction`, validate each cart line against fresh `Product.StockQuantity`, decrement stock on success, and return a new `InsufficientStock` result flag on failure with no order/stock side-effects — verify with unit test
- [x] 4.2 Handle `InsufficientStock` in `CartController.SummaryPOST`: redirect to `Cart/Index` with localized `TempData["error"]`, no `SessionId`/`OrderHeader` created — verify via integration test
- [x] 4.3 Ensure Company delayed-payment checkout also validates and decrements stock in the same transaction — verify with a company-role integration test
- [x] 4.4 Add `CHECK (StockQuantity >= 0)` DB constraint (via migration annotation) as a safety net and verify it appears in the generated migration SQL

## 5. Tests & Verification

- [x] 5.1 Add `VaultShop.Tests/StockInventoryTests.cs` (or extend `CheckoutServiceTests`) covering: add-to-cart exceeds stock, Plus at limit, checkout success decrements, checkout single-line failure rolls back, concurrent-checkout not-negative invariant (sequential simulation) — verify `dotnet test` passes
- [x] 5.2 Extend `CartCheckoutHttpTests` (or add HTTP test) for insufficient-stock redirect path — verify `dotnet test` passes
- [x] 5.3 Run `dotnet test VaultShop.sln` full suite green and `dotnet build --no-restore` clean before marking change ready

## 6. Review Fixes (post-review corrections before archive)

- [x] 6.1 Backfill existing stock: add `UPDATE "Products" SET "StockQuantity" = 10 WHERE "IsDeleted" = false` via migration (or idempotent `DbInitializer` seed) so pre-migration products with availability are sellable after deploy — verify `SELECT COUNT(*) FROM "Products" WHERE "IsDeleted" = false AND "StockQuantity" = 0` returns 0 post-migration
- [x] 6.2 Map `DbUpdateException` from the `CK_Products_StockQuantity_NonNegative` check constraint (in `CheckoutService.CreateOrder`, inside the transaction) to `InsufficientStock = true` with a `Warning`-level log instead of surfacing a 500 — verify concurrent second checkout on last unit redirects to cart with localized error (integration test forcing the constraint, e.g. Testcontainers PG or direct constraint violation)
- [x] 6.3 Fix `CartController.Plus` guard: filter `Product.Get(u => u.Id == cart.ProductId && !u.IsDeleted && u.IsAvailableInStore)` and treat `product == null` as insufficient stock (localized error, no increment) — verify with a unit test mirroring `HomeControllerAddToCartStockTests`
- [x] 6.4 Reject tampered quantities: in `HomeController.Details` POST, explicitly reject `shoppingCart.Count < 1` (before the stock check) with localized error + redirect — verify `Count = 0` and `Count = -5` POSTs do not mutate cart/stock
- [x] 6.5 Add boundary theory tests to `HomeControllerAddToCartStockTests`: `(existing:0, requested:1, stock:1, succeeds)` / `(existing:2, requested:1, stock:2, rejected)` / `(existing:0, requested:1, stock:0, rejected)` plus `Plus_BelowLimit_Increments` and `Product_StockQuantity_Negative_FailsValidation` — verify `dotnet test` passes
