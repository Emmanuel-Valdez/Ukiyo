## Context

Orders are already persisted with header totals, customer/company data, and payment state (see `OrderHeader`, checkout, and Phase 3 email models). There is currently no billing artifact and no captured fiscal data. This change adds the minimum billing level on top of the existing order model without altering payment or order-state behavior.

## Goals / Non-Goals

**Goals:**
- Choose and document the minimum billing level for launch.
- Keep internal receipts clearly distinct from legal invoices.
- Guarantee billing totals/data consistency with the order.
- Leave a clean extension point for a future fiscal provider.

**Non-Goals:**
- Implementing real fiscal invoicing (AFIP/AR CAE) now.
- Replacing or changing payment methods or order-state transitions.
- Capturing full fiscal customer data unless the chosen level requires it.

## Decisions

- **Start with internal receipt only.** A real legal invoice in Argentina (factura electrónica AFIP) is out of launch scope; overbuilding fiscal automation early is a roadmap risk. The receipt is explicitly non-legal.
  - Alternative considered: implement fiscal invoicing now — rejected to avoid blocking launch and untried provider coupling.
- **Receipt reads from `OrderHeader` persisted values**, not recomputed from cart, so it matches confirmation emails and admin views by construction.
- **No new billing tables yet.** Fiscal fields are added later as nullable columns when a provider is chosen, preserving current models (satisfies the "not blocked" requirement).

## Risks / Trade-offs

- [Risk] Users or admins may treat the internal receipt as a legal invoice → Mitigation: explicit non-legal labeling in UI, email, and document text per the spec.
- [Risk] Insufficient fiscal data captured now forces a later migration → Mitigation: keep models open (nullable fiscal columns) and document the assumption in plans/.
- [Risk] Inconsistent totals if receipt logic diverges from email/order → Mitigation: single source is `OrderHeader`; no independent total computation.
