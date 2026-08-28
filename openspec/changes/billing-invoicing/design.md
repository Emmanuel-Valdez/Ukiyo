## Context

Verified current state (2026-08-26 code review): orders persist header totals (`OrderHeader.OrderTotal`), line prices/counts (`OrderDetail.Price`, `OrderDetail.Count`, snapshotted at checkout in `CheckoutService.CreateOrder`), customer/address data, company linkage (`OrderHeader.CompanyId`), and payment state. `Company` has only name, generic address, phone, soft-delete flag — no fiscal fields anywhere. Order access is guarded by `UserCanAccessOrder` (admin/staff pass; otherwise owner or same-company match), returning 404 for foreign orders. There is no server-side PDF capability (no PDF package in any project); the only PDF output is client-side pdfmake used by DataTables exports. The email abstraction is a 3-string `IEmailSender` with no attachment support. No billing artifact exists.

## Goals / Non-Goals

**Goals:**
- Ship the minimum useful commercial document: "Resumen de pedido", viewable as HTML and downloadable as PDF from inside the app.
- Capture the fiscal data wholesale customers actually need now: razón social, domicilio fiscal, optional CUIT.
- Guarantee totals/data consistency by construction (single source, no recomputation).
- Leave future fiscal integration (ARCA/CAE) addable without touching order-domain concepts.

**Non-Goals:**
- Real fiscal invoicing (AFIP/AR CAE, factura A/B/C, fiscal numbering, fiscal QR).
- Emailing the document or adding attachment support to `IEmailSender`.
- Public token-based URLs for unauthenticated summary access.
- Replacing client-side pdfmake table exports.

## Decisions

- **Billing level = internal commercial document ("Resumen de pedido"), not a receipt and never an invoice.** Legal invoicing is deferred until Ukiyo needs it; ARCA coupling before launch was rejected as roadmap risk. Every representation carries the localized non-fiscal legend; final legal wording is explicitly deferred to later review and must not invent fiscal requirements.
- **Fiscal data lives on `Company`, snapshot onto `OrderHeader`.** Nullable columns `RazonSocial`, `DomicilioFiscal`, `Cuit` (optional) added to `Company`; admins edit them via the existing Company upsert. At checkout, company orders copy the values onto matching nullable `OrderHeader` columns — the same historical-snapshot pattern already used for addresses — so later Company edits never rewrite past documents. Customer (non-company) orders leave the snapshot null.
- **One model, two representations.** `OrderSummaryService` maps persisted `OrderHeader` + `OrderDetail`s into an `OrderSummaryViewModel` (order id/date, status, payment method/status, items with unit price/count/line total, `OrderTotal`, currency context, customer/company data incl. fiscal snapshot, delivery address). The HTML view and the QuestPDF generator both consume that view model. Nothing recomputes totals, mirroring how Phase 3 emails already read `OrderHeader`.
- **PDF via QuestPDF (Community License).** New package; license configured once at startup. The generator produces the same table content as the HTML view. Dockerfile gains the native library QuestPDF requires on Linux (verified by container smoke test in tasks). Alternative considered: reusing client-side pdfmake — rejected because a real downloadable file matters for wholesale clients forwarding to accountants, and pdfmake output quality/layout control is weaker.
- **Access control reuses the existing guard.** Summary view/download routes call the same ownership logic as order Details (`UserCanAccessOrder` pattern): admin/employee allowed, owner allowed, same-company users allowed, everyone else gets 404, anonymous gets the login challenge. No new authorization surface, no public tokens (decided out of scope).
- **Naming stays neutral.** Code identifiers, routes, resources, and UI copy use "Order Summary"/"Resumen de pedido"; the words "factura"/"invoice" appear only inside the non-fiscal legend itself. This keeps search-and-replace safe when a future `FiscalDocument` capability arrives as an independent feature referencing the order.

## Risks / Trade-offs

- [Risk] Users treat the resumen as a legal invoice → Mitigation: legend on every representation, neutral naming, spec scenario forbidding invoice wording elsewhere.
- [Risk] Fiscal wording may not satisfy a future legal review → Mitigation: legend text centralized in localization resources so wording changes are one-file edits; documented assumption in plans/.
- [Risk] Inconsistent totals between HTML/PDF/order → Mitigation: single view model sourced from `OrderHeader`; unit test asserts rendered values equal persisted order.
- [Risk] QuestPDF native dependency breaks the Linux container → Mitigation: explicit Dockerfile change plus container-level smoke test before merge.
- [Trade-off] CUIT captured as free text (optional), no format enforcement beyond length/basic characters — formal validation belongs to the future fiscal capability, which will own stricter rules.
