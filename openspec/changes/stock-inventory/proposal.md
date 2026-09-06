## Why

`Product` has no stock field and cart/checkout never validate quantity, so the store can oversell with no admin visibility. This is Verified Gaps Backlog #1 (launch-risk) — every other Phase 11 hardening item is done, and stock/inventory is the remaining blocker before the catalog can scale safely.

## What Changes

- Add `Product.StockQuantity` (int, >= 0, default 0) via EF Core migration; seed existing products to a safe initial value and keep `IsDeleted`/`IsAvailableInStore` semantics unchanged.
- Admin can view and edit stock: show Stock in `ProductController.GetAll` (admin product table) and add a required stock input to `ProductController.Upsert` (create/edit) with server-side validation.
- Storefront guards: `HomeController.Details` POST (add-to-cart) and `CartController.Plus` validate requested quantity against available stock and reject with a localized error instead of silently overselling.
- Checkout guard: `CheckoutService.CreateOrder` validates the entire cart against current stock inside `ExecuteInTransaction`, decrements `Product.StockQuantity` atomically when the order is created, and fails the checkout with a cart-friendly error if any line exceeds stock (no partial order).
- Cart/checkout UX: quantity stepper and cart pages respect stock limits (max = available stock); out-of-stock products show as unavailable and cannot be added.
- No reservation/hold system, no low-stock threshold, no automatic restock, no stock history — stock is a single scalar decremented only at order creation.

## Capabilities

### New Capabilities
- `stock-inventory`: Product stock quantity, admin stock management, add-to-cart/cart/checkout stock validation, and atomic decrement at order creation.

### Modified Capabilities
- _(none — additive; existing checkout/order specs unchanged, stock-inventory adds its own rules)_

## Impact

- **Models/Data:** `VaultShop.Models/Product.cs`, `VaultShop.DataAccess` migration + `ApplicationDbContextModelSnapshot`, `VaultShop.DataAccess/DbInitializer` if seed data touched.
- **Admin:** `VaultShop.Web/Areas/Admin/Controllers/ProductController.cs`, `Areas/Admin/Views/Product/Upsert.cshtml`, `Areas/Admin/Views/Product/Index.cshtml` (DataTables via `GetAll` JSON), `ProductVM` if needed, localization resx.
- **Storefront/Checkout:** `VaultShop.Web/Areas/Customer/Controllers/HomeController.cs`, `VaultShop.Web/Areas/Customer/Controllers/CartController.cs`, `VaultShop.Web/Services/Checkout/CheckoutService.cs` (+ `ICheckoutService` result types if needed), `Areas/Customer/Views/Home/Details.cshtml`, `Areas/Customer/Views/Cart/Index.cshtml` + Summary view, `wwwroot/js` stepper if capped.
- **Tests:** `VaultShop.Tests` — new `StockInventoryTests` + updates to `CheckoutServiceTests`/`CartCheckoutHttpTests`.
- **Dependencies:** None — uses existing EF Core/Npgsql, no new packages.
- **Risk:** Low — single column + guards; concurrency race is closed by the checkout transaction (DB read inside transaction). Deferred: low-stock alerts, stock history, reservations, backorders.
