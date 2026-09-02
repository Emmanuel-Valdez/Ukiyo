## Why

The store takes paid orders but produces no commercial document. Wholesale (Company) customers need a printable, downloadable order summary with their company data for their own records and accountants. Legal fiscal invoicing in Argentina (ARCA/AFIP) is explicitly out of launch scope, so the first billing artifact must be an internal commercial document that can never be mistaken for a factura.

## What Changes

- Define and document the billing level: an internal commercial document named **"Resumen de pedido"** (Order Summary), explicitly NOT a fiscal document.
- Capture company fiscal data: `RazonSocial` and `DomicilioFiscal` required for companies, `Cuit` optional. Managed on the Company entity by admins; snapshotted onto the order at checkout for historical consistency (same pattern as address snapshots).
- Add an `OrderSummaryService` that builds a single `OrderSummaryViewModel` from persisted `OrderHeader`/`OrderDetail` values (no total recomputation).
- Render the summary as an HTML view (table layout) linked from the order detail views available to customers, company users, and admins.
- Add PDF download from within the app via QuestPDF (server-side), consuming the same view model.
- Carry the explicit non-fiscal legend on every representation: "DOCUMENTO NO VÁLIDO COMO FACTURA NI COMO COMPROBANTE FISCAL." (localized es/en; final wording subject to later legal review).

## Capabilities

### New Capabilities
- `billing-invoicing`: internal commercial order summary generation (HTML + PDF from one model), clear commercial-vs-fiscal distinction, company fiscal data capture and snapshotting, authenticated access control, and a non-blocking path for future fiscal integration.

### Modified Capabilities
<!-- None: payments, orders, checkout state transitions, and transactional emails keep current behavior. -->

## Impact

- Models/migrations: nullable `RazonSocial`, `DomicilioFiscal`, `Cuit` on `Company`; matching nullable snapshot columns on `OrderHeader`.
- Admin Company upsert (controller + view): new fiscal fields with validation and localization.
- Checkout (`CheckoutService.CreateOrder`): copies company fiscal snapshot onto company orders.
- New services: `OrderSummaryService` (+view model), QuestPDF document generator.
- Views: order summary HTML view; links from shared order Details and OrderConfirmation pages; localization resources (en/es).
- Infrastructure: QuestPDF package + community license setting; Dockerfile gains the native library QuestPDF needs; existing client-side pdfmake exports remain untouched.

## Out of Scope

- Fiscal/legal invoicing: CAE, ARCA integration, factura A/B/C, fiscal point-of-sale/numbering, fiscal QR, itemized IVA presented as an invoice, or any mechanism presenting this document as fiscal.
- Emailing the summary or PDF; adding attachment support to the email abstraction; changes to confirmation email content.
- Public unauthenticated token-based summary URLs.
- Migrating existing client-side pdfmake table exports to server-side PDF.
- Stock, shipping cost lines, discounts/coupons, customer self-cancel (separate future changes).
