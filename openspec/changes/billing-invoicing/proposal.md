## Why

Billing depends on real payment methods, order states, customer data, and Ukiyo legal requirements. Today the store can take paid orders but has no defined billing artifact: there is no clear operational distinction between an internal order receipt and a legal invoice, and customer fiscal data is not captured at checkout. We need the minimum billing level now, with a path open for future fiscal-provider integration, without overclaiming fiscal compliance.

## What Changes

- Define and document the chosen billing level (internal receipt vs. real invoice vs. future fiscal integration).
- If an internal receipt is implemented, it MUST be clearly distinguished from a legal invoice.
- Capture and validate the minimum customer/company fiscal fields needed at checkout for the chosen level.
- Ensure order/customer totals and data are consistent across receipt generation, order detail views, and emails.
- Keep naming and models open for a future fiscal-provider integration.

## Capabilities

### New Capabilities
- `billing-invoicing`: the minimum billing artifact for Ukiyo/VaultShop orders — internal receipt generation, clear internal-receipt-vs-legal-invoice distinction, consistent totals, and a non-blocking path for future fiscal integration.

### Modified Capabilities
<!-- No existing capability requirement changes; payments and orders keep their current behavior. -->

## Impact

- Order models and order detail views (customer and admin).
- Admin order pages.
- Email templates (receipt/confirmation content).
- Possible future receipt/document generation service if the chosen level requires it.
- Checkout: customer fiscal fields may be added (gated by the chosen billing level).
