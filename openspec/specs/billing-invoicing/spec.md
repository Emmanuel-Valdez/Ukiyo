# billing-invoicing Specification

## Purpose

Defines the minimum billing artifact for VaultShop/Ukiyo orders: an internal commercial "Resumen de pedido" (HTML + PDF from one data model), company fiscal data capture, a clear commercial-vs-fiscal distinction, authenticated access control, and a non-blocking path for future fiscal integration.

## Requirements

### Requirement: Billing level is documented
The system SHALL document the chosen billing level — an internal commercial order summary ("Resumen de pedido") that is not a fiscal document — and the legal-status implication of that choice.

#### Scenario: Billing level recorded
- **WHEN** the billing capability is implemented or reviewed
- **THEN** `plans/` states that the chosen level is an internal commercial summary, explicitly not valid as a factura or fiscal receipt, with fiscal invoicing deferred

### Requirement: Company fiscal data is captured and snapshotted
The system SHALL allow admins to store razón social and domicilio fiscal on a company (both required), with CUIT optional, and SHALL snapshot those values onto company orders at checkout so later company edits do not alter past documents.

#### Scenario: Admin stores company fiscal data
- **WHEN** an admin saves a company without razón social or domicilio fiscal
- **THEN** validation rejects the save; saving with both fields and any optional CUIT persists them

#### Scenario: Checkout snapshots fiscal data
- **WHEN** a company user places an order and their company has fiscal data stored
- **THEN** the created order carries a copy of razón social, domicilio fiscal, and CUIT as of purchase time

#### Scenario: Customer orders have no fiscal snapshot
- **WHEN** a non-company customer places an order
- **THEN** the created order's fiscal snapshot fields remain empty

### Requirement: Order summary reflects persisted order data
The order summary (HTML and PDF) SHALL be generated exclusively from persisted order values — header totals, line prices/counts, status, payment method/status, customer/company data including the fiscal snapshot, and delivery address — without recomputing totals.

#### Scenario: Summary matches the persisted order
- **WHEN** a summary is rendered or generated for an order
- **THEN** its items, unit prices, quantities, line totals, grand total, currency context, statuses, and customer/company/fiscal data equal the persisted order values

#### Scenario: Both order kinds render
- **WHEN** a summary is requested for a Customer order and for a Company order
- **THEN** both render with consistent structure; the company one includes the fiscal data snapshot

### Requirement: The summary is clearly not a fiscal document
Every representation of the order summary SHALL display the localized legend equivalent to "DOCUMENTO NO VÁLIDO COMO FACTURA NI COMO COMPROBANTE FISCAL." and SHALL NOT use legal-invoice terminology anywhere else in its content.

#### Scenario: Legend present in both languages
- **WHEN** the summary HTML view or downloaded PDF is viewed with culture es-AR or en-US
- **THEN** the non-fiscal legend appears in the active language and no invoice/factura wording appears outside the legend

### Requirement: Authenticated access control on summaries
The system MUST prevent unauthenticated users and users without access from viewing or downloading an order summary; access SHALL follow the existing order ownership rules (admin/employee, order owner, same-company user), denying others without leaking existence.

#### Scenario: Foreign order denied
- **WHEN** an authenticated user requests the summary of an order they neither own nor belong to via company
- **THEN** the response is 404

#### Scenario: Anonymous redirected
- **WHEN** an anonymous visitor requests a summary view or download
- **THEN** they are challenged to log in before any data is shown

### Requirement: PDF download from within the app
The system SHALL offer a PDF download of the order summary to authorized users from the app, produced server-side from the same summary model as the HTML view.

#### Scenario: Authorized download succeeds
- **WHEN** an authorized user triggers the PDF download for an order
- **THEN** the response is a `application/pdf` stream whose content matches the persisted order data and whose filename identifies the order

### Requirement: Transactional emails stay unchanged
The confirmation email flow SHALL NOT change: no summary/PDF attachment is added and no email content is rewritten by this capability.

#### Scenario: Confirmation email unchanged
- **WHEN** an order is created after this change
- **THEN** the confirmation email has the same content structure as before, with no attached document

### Requirement: Future fiscal integration is not blocked
The order and company models and the summary naming SHALL NOT prevent adding a future independent fiscal-document capability (e.g., ARCA/CAE) referencing orders.

#### Scenario: Fiscal capability can extend later
- **WHEN** a future fiscal document feature is designed
- **THEN** it can be added as an independent capability related to the order id, without renaming summary artifacts or breaking existing summary data
