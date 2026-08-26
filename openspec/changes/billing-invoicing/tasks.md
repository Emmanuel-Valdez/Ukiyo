## 1. Decision & Documentation

- [ ] 1.1 Decide and record the chosen billing level (internal receipt) and its non-legal status in the Phase 9 section of `plans/`, and verify the choice and legal implication are stated.

## 2. Receipt Generation

- [ ] 2.1 Implement an internal receipt model/view that reads totals, currency, and customer/company data from `OrderHeader` (no independent total computation), and verify a unit test asserts the rendered values equal the persisted order.
- [ ] 2.2 Render the receipt with explicit non-legal "internal receipt" labeling in UI and email output, and verify the wording is present and contains no legal-invoice terms.

## 3. Order Views

- [ ] 3.1 Surface the receipt on customer and admin order detail views, and verify it renders for both Customer and Company orders with consistent data.

## 4. Consistency

- [ ] 4.1 Confirm the receipt and Phase 3 confirmation emails both source totals from `OrderHeader` with no duplicated total logic, and verify by code review plus the existing email/order tests.

## 5. Verification

- [ ] 5.1 Run `dotnet test VaultShop.sln` and confirm green; manually check one Customer and one Company receipt in local/fake mode.
