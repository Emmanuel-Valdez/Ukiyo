## Purpose

Defines the minimum billing artifact for VaultShop/Ukiyo orders, keeping a clear internal-receipt-vs-legal-invoice distinction and a non-blocking path for future fiscal integration.

## ADDED Requirements

### Requirement: Billing level is documented
The system SHALL document the chosen billing level (internal receipt, legal invoice, or future fiscal integration) and the legal-status implication of that choice.

#### Scenario: Billing level recorded
- **WHEN** the billing capability is configured or implemented
- **THEN** the selected level and its legal status are recorded in project documentation (plans/ or the capability spec)

### Requirement: Internal receipt is not a legal invoice
If the system generates an internal order receipt, it SHALL NOT label or present that receipt as a legal invoice unless it legally is one.

#### Scenario: Receipt marked non-legal
- **WHEN** an internal receipt is shown or emailed to a customer
- **THEN** the artifact is clearly identified as a non-lebgal internal receipt and omits any legal-invoice wording

### Requirement: Consistent billing totals and data
The billing artifact SHALL reflect the order header totals and customer/order data that are consistent with the order record and any receipt or confirmation emails.

#### Scenario: Totals match the order
- **WHEN** a billing artifact is generated for an order
- **THEN** its totals, currency, and customer/company data equal the persisted order values

### Requirement: Future fiscal integration is not blocked
The order and customer data model and the billing naming SHALL NOT prevent a future fiscal-provider integration (e.g. AFIP/AR CAE).

#### Scenario: Model accommodates later fiscal fields
- **WHEN** a future fiscal integration is added
- **THEN** it can extend the existing order/customer models and billing naming without a breaking rename or data migration that conflicts with current artifacts
