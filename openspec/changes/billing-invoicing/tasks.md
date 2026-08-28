## 1. Decision & Documentation

- [x] 1.1 Record the chosen billing level (internal commercial "Resumen de pedido", non-fiscal) and the approved decisions (no email delivery, no public token URL, QuestPDF, fiscal fields razón social + domicilio fiscal required / CUIT optional) in the Phase 9 section of `plans/`, and verify the choice and legal implication are stated.
- [x] 1.2 Record the post-Phase-9 exploration backlog (stock, security hardening, payment reconciliation, health checks, sitemap fix, later items) under Phase 11 candidates in `plans/`, and verify each gap names its evidence.

## 2. Data & Migration

- [x] 2.1 Add an EF Core migration with nullable `RazonSocial`, `DomicilioFiscal`, `Cuit` on `Company` and matching nullable snapshot columns on `OrderHeader`, and verify it applies cleanly to a local database.
- [x] 2.2 Extend admin Company upsert (model binding, validation: razón social and domicilio fiscal required, CUIT optional) with en/es localization, and verify save/reject behavior in the browser.
- [x] 2.3 Copy company fiscal data onto `OrderHeader` in `CheckoutService.CreateOrder` for company orders only, and verify a test asserts the snapshot is written for company orders and left empty for customer orders.

## 3. Summary Generation

- [x] 3.1 Implement `OrderSummaryService` producing an `OrderSummaryViewModel` exclusively from persisted `OrderHeader`/`OrderDetail` values (items, totals, statuses, customer/company/fiscal snapshot, address), and verify a unit test asserts rendered values equal the persisted order for one Customer order and one Company order.

## 4. HTML Presentation

- [x] 4.1 Render the summary view as a table layout (order number, date, customer/company + fiscal data, items with unit price/quantity/line total, shipping/delivery data, total, payment method/status, order status, non-fiscal legend), localized en/es, working in light and dark mode.
- [x] 4.2 Link the summary from the shared order Details view (visible to customers, company users, admins) and from the OrderConfirmation page, respecting existing access guards, and verify a foreign-order request returns 404 and anonymous access challenges to login.

## 5. PDF Download

- [x] 5.1 Add QuestPDF with community license configuration and a document generator consuming the same `OrderSummaryViewModel`, including the localized non-fiscal legend, and verify generated output matches the HTML data.
- [x] 5.2 Add a guarded download endpoint returning `application/pdf` with an order-identifying filename, and verify authorized download succeeds and denied roles get 404.
- [x] 5.3 Update the Dockerfile with the native library QuestPDF needs on Linux, and verify PDF generation inside the container. (No apt/system packages required; `SkiaSharp.NativeAssets.Linux.NoDependencies` bundles native libs. Verified by generating a PDF inside the final runtime container and by running the existing PDF generator test.)

## 6. Consistency & Verification

- [x] 6.1 Confirm summary, order Details, and Phase 3 emails all source totals from `OrderHeader` with no duplicated total logic, and verify by code review plus existing email/order tests.
- [x] 6.2 Run `dotnet test VaultShop.sln` green; manually check one Customer and one Company order (HTML + PDF) in local/fake mode across es-AR/en-US and light/dark themes.

