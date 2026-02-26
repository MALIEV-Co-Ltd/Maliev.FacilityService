# Feature Specification: Facility & Equipment Management

**Feature Branch**: `001-equipment-management`
**Created**: 2026-02-25
**Revised**: 2026-02-25
**Status**: Draft
**Input**: Full company asset lifecycle management — all physical equipment owned by MALIEV, including manufacturing machines (FDM, SLA, CNC, 3D scanners, injection moulding) and general assets (office equipment, IT equipment, measuring tools, hand tools, and other).

---

## Scope

`Maliev.FacilityService` is the **single source of truth for all physical assets owned by MALIEV**. It manages the full lifecycle of every piece of company equipment — from registration and asset tracking through lending, maintenance logging, and eventual decommissioning.

### In Scope

- Registration and management of all equipment categories
- System-generated asset codes (`MAL-{PREFIX}-{SEQ}`) and manufacturer serial number tracking
- Brand and model name capture for all equipment
- Equipment lending to employees (informal assignment) and customers (approval-required, document-backed)
- Equipment maintenance log (per-event history, vendor tracking)
- CNC machine durable fixtures and attachments (vises, tool holders, fixtures)
- Equipment notes (append-only per-equipment log, immutable entries)
- Status lifecycle: Active ↔ UnderMaintenance ↔ OnLoan; Active/UnderMaintenance → Lost → Active (recovery); any non-terminal → Decommissioned (terminal)
- Optimistic concurrency control on all mutable records
- `EquipmentStatusChangedEvent` published via MassTransit outbox on every status transition
- `LoanDocumentRequestedEvent` published to `Maliev.PdfService` when a customer loan is approved
- Query endpoints for PricingService (manufacturing machine specs + rates) and JobService (available machines by category)

### Out of Scope

- **Consumables** (filament, resin, scanning spray, marker dots, cutting tools/inserts, coolant) → `InventoryService`
- **Material process parameters** (FDM layer times, SLA exposure times, resin profiles) → `MaterialService`
- **Job lifecycle management** → `JobService`
- **Pricing formula computation** → `PricingService`
- **PDF rendering** → `Maliev.PdfService` (FacilityService publishes structured data; PdfService renders the document)
- Customer-facing access of any kind
- Vendor/external-technician user accounts

---

## User Scenarios & Testing

### User Story 1 — Register Equipment (Priority: P1)

An employee opens the equipment registration page on the Intranet. They select a category (e.g. FDM Printer), enter the brand ("Bambu Lab"), model name ("X1C"), a human-readable name ("Bambu X1C #2"), manufacturer serial number, purchase date, and purchase price. For manufacturing machines, they also enter typed spec fields (build volume, hourly rate, setup fee, spindle speed, etc.). The system generates a unique asset code (e.g. `MAL-FDM-0003`) and saves the record with `Active` status.

**Why this priority**: No downstream feature (lending, maintenance, pricing, job assignment) can operate without registered equipment. This is the entry point for the entire asset lifecycle.

**Independent Test**: Register one FDM printer and one office printer → both appear in the equipment list with correct asset codes, types, and Active status.

**Acceptance Scenarios**:

1. **Given** no FDM printers exist, **When** an employee registers a valid FDM printer with all required spec fields, **Then** the machine is saved with `Status = Active`, asset code `MAL-FDM-0001` is auto-generated, and the record appears in the equipment list.
2. **Given** an employee registers an office printer (category: OfficeEquipment), **When** they submit without build volume or hourly rate, **Then** the system accepts the record — those fields are not required for general equipment.
3. **Given** an employee submits a registration for a CNC machine without the required `XTravelMm` field, **When** they submit, **Then** the system rejects the form with a validation message identifying the missing field.
4. **Given** two registrations use the same `Name` field, **When** the second is submitted, **Then** the system rejects it with a duplicate-name conflict error.
5. **Given** an FDM printer is registered, **When** the record is saved, **Then** the asset code is immutable — no further edit can change it.
6. **Given** an employee registers a CNC machine with `ToolInterface = BT40`, **When** the record is saved, **Then** `ToolInterface` is stored and returned in the machine detail.
7. **Given** an employee registers a 3D scanner, **When** they submit with `ScannerTechnology = StructuredLight` and typed fields (accuracy, max scan volume, resolutions), **Then** all fields are persisted correctly.

---

### User Story 2 — Query Manufacturing Equipment for Pricing (Priority: P2)

PricingService queries all Active manufacturing equipment of a given category type and receives their typed spec fields, hourly rates, and extended properties to compute a deterministic quote.

**Why this priority**: PricingService cannot produce correct quotes without live machine data. This directly impacts revenue correctness.

**Independent Test**: Seed one Active FDM printer and one UnderMaintenance FDM printer → query for Active FDM → only the Active machine returned with full spec shape, including `isOutsourced: false`.

**Acceptance Scenarios**:

1. **Given** two Active FDM printers exist, **When** PricingService queries for Active FDM machines, **Then** both are returned with all typed spec fields (build volume, nozzle diameter, layer height range, etc.) and `isOutsourced: false`.
2. **Given** no Active CNC machines exist, **When** PricingService queries for Active CNC machines, **Then** the response is `{ "items": [], "isOutsourced": true }` — never an error.
3. **Given** an FDM machine is `UnderMaintenance`, **When** PricingService queries for Active FDM machines, **Then** the under-maintenance machine is excluded.
4. **Given** an FDM machine is `OnLoan`, **When** PricingService queries for Active FDM machines, **Then** the on-loan machine is excluded.
5. **Given** an FDM machine is `Lost`, **When** PricingService queries for Active FDM machines, **Then** the lost machine is excluded.

---

### User Story 3 — Update Equipment Status (Priority: P3)

An employee changes a machine's operational status (e.g. Active → UnderMaintenance). The change is saved immediately, reflected in all subsequent queries, and a status-change event is published.

**Why this priority**: Status control is the core operational gate for the workshop floor. Incorrect status causes mis-routing of jobs and pricing errors.

**Independent Test**: Set a machine to UnderMaintenance → confirm excluded from active queries → restore to Active → confirm re-appears → set to Decommissioned → attempt to restore → system rejects with 422.

**Acceptance Scenarios**:

1. **Given** a machine is Active, **When** an employee changes status to UnderMaintenance, **Then** the machine is excluded from active/pricing/job queries and `EquipmentStatusChangedEvent` is published.
2. **Given** a machine is UnderMaintenance, **When** an employee restores it to Active, **Then** it re-appears in active queries and `EquipmentStatusChangedEvent` is published.
3. **Given** a machine is OnLoan, **When** the loan is returned, **Then** the machine status returns to Active and `EquipmentStatusChangedEvent` is published.
4. **Given** a machine is Lost and subsequently found, **When** an employee sets status to Active, **Then** the machine is restored and the event is published.
5. **Given** a machine is Decommissioned, **When** an employee attempts any status change, **Then** the system rejects the request with HTTP 422.
6. **Given** two employees simultaneously update the same machine's status, **When** both submit, **Then** the second receives a 409 conflict error.

---

### User Story 4 — View and Filter Equipment List (Priority: P4)

An employee browses all registered assets, filtering by category, status, or free-text search. Pagination is supported.

**Why this priority**: Asset visibility is core to daily operations. Without filtering, employees cannot efficiently manage a growing asset list.

**Independent Test**: Seed machines of multiple categories and statuses → filter by category → filter by status → combine filters → verify correct results at each step.

**Acceptance Scenarios**:

1. **Given** mixed categories exist, **When** an employee filters by `FdmPrinter`, **Then** only FDM printers are shown.
2. **Given** mixed statuses exist, **When** an employee filters by `UnderMaintenance`, **Then** only those machines are shown.
3. **Given** combined filters (FdmPrinter + Active), **When** applied, **Then** only Active FDM printers are shown.
4. **Given** no assets match the filter, **When** applied, **Then** an empty page is returned — not an error.
5. **Given** more than `pageSize` assets exist, **When** the first page is requested, **Then** only `pageSize` items are returned with a correct `totalCount`.

---

### User Story 5 — Update Equipment Details (Priority: P5)

An employee updates a machine's name, brand, model, purchase price, spec fields (build volume, hourly rate, etc.), or extended properties. Optimistic concurrency prevents silent overwrites.

**Why this priority**: Calibration and correction are routine operations. Pricing accuracy depends on up-to-date spec data.

**Independent Test**: Update HourlyRateTHB → verify new value returned in next query. Attempt same update twice with stale rowVersion → 409 on second attempt.

**Acceptance Scenarios**:

1. **Given** a machine's HourlyRateTHB is 150, **When** an employee updates it to 180, **Then** the new rate is immediately returned by PricingService queries.
2. **Given** two employees attempt the same update simultaneously, **When** both submit, **Then** the second receives a 409 conflict.
3. **Given** an FDM machine's `NozzleDiameterMm` is updated, **When** the change is saved, **Then** the new value appears in both the detail view and pricing query responses.
4. **Given** an employee renames a machine to a name already in use by another machine, **When** they submit, **Then** the system rejects with a 409 conflict.

---

### User Story 6 — Add Equipment Notes (Priority: P6)

An employee adds a timestamped note to an equipment record (e.g. "Nozzle replaced 25/02/2026"). Notes are append-only — existing notes cannot be edited or deleted.

**Why this priority**: Notes provide an informal audit trail for maintenance decisions, observations, and handover information.

**Independent Test**: Add two notes to a machine → list notes → both appear in chronological order with author and timestamp.

**Acceptance Scenarios**:

1. **Given** a machine has no notes, **When** an employee adds a note, **Then** the note appears in the machine's note list with the author's employee ID and UTC timestamp.
2. **Given** a machine has existing notes, **When** a second employee adds another note, **Then** both notes appear in chronological order.
3. **Given** an existing note, **When** an employee attempts to edit or delete it, **Then** the system rejects the request — notes are immutable.

---

### User Story 7 — Lend Equipment to an Employee (Priority: P7)

A manager assigns a piece of equipment to an employee (e.g. a measuring tool or a laptop). The equipment status changes to `OnLoan`, is excluded from availability queries, and the loan record captures who has it and for how long.

**Why this priority**: Employee assignments are the most common form of internal asset tracking. Without this, equipment location is unknown.

**Independent Test**: Assign a caliper to an employee → verify status is OnLoan → verify equipment excluded from active queries → record return → status returns to Active.

**Acceptance Scenarios**:

1. **Given** an Active piece of equipment, **When** a manager creates an employee loan record, **Then** the equipment status changes to OnLoan immediately (no approval required).
2. **Given** an OnLoan piece of equipment, **When** the employee returns it, **Then** the loan is marked Returned, the equipment status returns to Active, and `EquipmentStatusChangedEvent` is published.
3. **Given** an OnLoan piece of equipment, **When** PricingService or JobService queries for active equipment, **Then** the on-loan machine is excluded.

---

### User Story 8 — Lend Equipment to a Customer (Priority: P8)

An employee creates a customer loan request (e.g. lending an injection moulding machine to a customer whose unit is under repair). A manager approves the request, the equipment status changes to `OnLoan`, and `LoanDocumentRequestedEvent` is published so PdfService generates a loan document.

**Why this priority**: Customer lending is an active operational need (injection moulding machine is currently on loan). Document trail is required for business and legal protection.

**Independent Test**: Create customer loan request (status: PendingApproval) → manager approves → status transitions to Active loan → equipment OnLoan → event published → record return → equipment Active.

**Acceptance Scenarios**:

1. **Given** an Active machine, **When** an employee creates a customer loan request, **Then** a LoanRecord with `LoanStatus = PendingApproval` is created and the equipment status remains Active (not yet on loan).
2. **Given** a PendingApproval loan request, **When** a manager approves it, **Then** the loan status becomes Active, equipment status changes to OnLoan, and `LoanDocumentRequestedEvent` is published.
3. **Given** an Active customer loan, **When** the customer returns the machine and an employee records the return with condition notes, **Then** the loan status becomes Returned and the equipment status returns to Active.
4. **Given** an Active customer loan, **When** the expected return date passes without return, **Then** the loan status automatically transitions to Overdue (background job or flag at query time).
5. **Given** a PendingApproval loan, **When** a manager rejects it, **Then** the loan request is cancelled and the equipment status is unchanged.

---

### User Story 9 — Log Equipment Maintenance (Priority: P9)

An employee or technician logs a maintenance event on a piece of equipment (scheduled service, unscheduled repair, or breakdown). The log captures what was done, who did it, any vendor involved, cost, and the next service due date.

**Why this priority**: Maintenance history is essential for compliance, total cost of ownership analysis, and service interval tracking.

**Independent Test**: Log a scheduled maintenance event → verify entry appears in maintenance history → next service due date updated on equipment record.

**Acceptance Scenarios**:

1. **Given** a machine exists, **When** a technician logs a maintenance event with type `Scheduled`, vendor name, description, and cost, **Then** the entry appears in the maintenance log with a UTC timestamp and the technician's ID.
2. **Given** a maintenance log entry includes a `NextServiceDueDate`, **When** the entry is saved, **Then** the equipment's `NextServiceDueDate` field is updated to that date.
3. **Given** a maintenance log entry exists, **When** any user attempts to edit it, **Then** the system rejects the request — maintenance log entries are immutable.
4. **Given** multiple maintenance events have been logged, **When** the log is listed, **Then** entries are returned in reverse-chronological order.

---

### User Story 10 — Manage CNC Machine Attachments (Priority: P10)

A manager registers durable CNC fixtures and attachments (vises, tool holders, fixtures) as child records of a CNC machine. Each attachment has a name, type, optional serial number, active/retired state, and condition notes.

**Why this priority**: Tracking fixtures is essential for CNC job scheduling — knowing which vises are available and in good condition affects what jobs can be assigned.

**Independent Test**: Add a vise to a CNC machine → verify it appears in the attachment list → mark it as retired → verify it is excluded from active attachment queries.

**Acceptance Scenarios**:

1. **Given** a CNC machine, **When** a manager adds a vise attachment with name, type, and serial number, **Then** the attachment appears in the machine's attachment list with `IsActive = true`.
2. **Given** an active attachment, **When** a manager marks it as retired with condition notes, **Then** `IsActive` becomes false and the condition notes are saved.
3. **Given** a non-CNC machine, **When** a manager attempts to add an attachment, **Then** the system rejects the request — attachments are only valid for CNC machines.

---

### User Story 11 — Hard Delete Equipment (Priority: P11)

An Admin permanently deletes an equipment record that has no job history and no active loans. The system verifies with JobService before proceeding.

**Why this priority**: Data hygiene — test records or mistaken registrations should be removable. History preservation is enforced by the job-history guard.

**Independent Test**: Register a machine, never use it in a job, attempt delete → 204. Register another, use in a job, attempt delete → 409.

**Acceptance Scenarios**:

1. **Given** a machine with no job history and no active loans, **When** an Admin deletes it, **Then** the record and all child records (notes, maintenance logs, attachments) are permanently removed.
2. **Given** a machine with job history, **When** an Admin attempts to delete it, **Then** the system returns 409 and suggests decommissioning.
3. **Given** JobService is unreachable, **When** an Admin attempts to delete any machine, **Then** the system returns 503 (fail-safe: block deletion under uncertainty).
4. **Given** a machine with an active loan, **When** an Admin attempts to delete it, **Then** the system rejects with 409 — active loans must be resolved first.

---

### User Story 12 — Query Available Machines for Job Assignment (Priority: P12)

JobService queries for all Active (not OnLoan, not Lost, not UnderMaintenance) manufacturing machines of a given category for job assignment routing.

**Why this priority**: JobService routing depends on live availability data. Without it, job assignment requires manual operator intervention.

**Independent Test**: Seed mixed machines → query Active FDM → only truly Active FDM machines returned.

**Acceptance Scenarios**:

1. **Given** three FDM printers (Active, UnderMaintenance, OnLoan), **When** JobService queries for available FDM machines, **Then** only the Active machine is returned.
2. **Given** no Active machines exist for a category, **When** JobService queries, **Then** `{ "items": [], "isOutsourced": true }` is returned with HTTP 200.
3. **Given** a Decommissioned machine, **When** JobService queries for available machines, **Then** it is never included.

---

## Edge Cases

- A machine is set to UnderMaintenance while a job is actively running on it — the status update is accepted; JobService decides whether to interrupt the in-progress job.
- Two employees simultaneously update the same equipment record — the second update receives a 409 conflict (xmin optimistic concurrency).
- An employee attempts to delete a machine with an active loan — the system rejects with 409 regardless of job history.
- A customer loan's expected return date passes — the loan transitions to Overdue; the equipment remains OnLoan.
- An employee registers a 3D scanner (no build volume) — the system accepts the record; build volume fields are not present on this type.
- An employee adds a CNC attachment to a 3D printer — the system rejects with 422.
- An employee attempts to recover a Lost machine by setting status to Active — the system permits this transition.
- PricingService queries a category with no active machines — returns `isOutsourced: true`, never an error.

---

## Requirements

### Functional Requirements

**Equipment Registration & Core Management**

- **FR-001**: Employees MUST be able to register equipment of any category. All registrations require: `Name` (unique), `Category`, `Brand?`, `ModelName?`, `ManufacturerSerialNumber?`, `PurchaseDate?`, `PurchasePriceTHB?`, `WarrantyExpiryDate?`. Manufacturing categories (FdmPrinter, SlaPrinter, CncMachine, Scanner3D, InjectionMolding) additionally require their typed spec fields. The system MUST auto-generate an immutable `AssetCode` in the format `MAL-{PREFIX}-{SEQ}` with a per-category sequence.
- **FR-002**: Employees MUST be able to update an equipment record's base fields and spec fields. `AssetCode` and `Category` are immutable after registration. Status changes are handled separately (FR-003).
- **FR-003**: Employees MUST be able to transition equipment status according to the permitted transition map. The system MUST reject illegal transitions with HTTP 422.
- **FR-004**: Employees MUST be able to view a paginated, filterable list of all equipment. Filters: `category`, `status`, `search` (free-text match on name/assetCode/brand/model). Default page size: 20, max: 100.
- **FR-005**: Admin employees MUST be able to permanently delete an equipment record only if: (a) JobService confirms no job history exists, and (b) no active loan exists. On deletion, all child records (notes, maintenance logs, loans, attachments) are cascade-deleted.
- **FR-006**: The system MUST reject hard deletion and return HTTP 409 when (a) JobService reports job history, or (b) an active loan exists. The system MUST return HTTP 503 when JobService is unreachable (fail-safe).

**Notes**

- **FR-007**: Employees MUST be able to append timestamped notes to any equipment record. Notes are immutable once created — no edit or delete is permitted.

**Status & Availability**

- **FR-008**: Equipment with status `Active` only is considered available for pricing and job assignment queries. Equipment with status `UnderMaintenance`, `OnLoan`, `Lost`, or `Decommissioned` MUST be excluded from all availability queries.
- **FR-009**: On every status transition, the system MUST publish `EquipmentStatusChangedEvent` via the MassTransit transactional outbox, carrying: `EquipmentId`, `AssetCode`, `Name`, `Category`, `PreviousStatus`, `NewStatus`.

**Manufacturing Equipment Queries**

- **FR-010**: The system MUST expose a query endpoint returning all `Active` manufacturing equipment of a given `Category`, with full typed spec fields, `HourlyRateTHB`, `SetupFeeTHB`, and `ExtendedProperties`. The response envelope MUST include `isOutsourced: false` when results exist and `isOutsourced: true` when the items array is empty.
- **FR-011**: When queried for a category with no active machines, the system MUST return HTTP 200 with `{ "items": [], "isOutsourced": true }` — never an error response.

**Lending**

- **FR-012**: Employees with `facility.loans.write` permission MUST be able to create an employee loan record, immediately transitioning equipment status to `OnLoan`. No approval required.
- **FR-013**: Employees with `facility.loans.write` permission MUST be able to create a customer loan request, creating a `LoanRecord` with `LoanStatus = PendingApproval`. Equipment status does not change at this point.
- **FR-014**: Employees with `facility.loans.approve` permission MUST be able to approve a `PendingApproval` customer loan. On approval: equipment status transitions to `OnLoan`, loan status becomes `Active`, and `LoanDocumentRequestedEvent` is published.
- **FR-015**: Employees with `facility.loans.approve` permission MUST be able to reject a `PendingApproval` customer loan. On rejection: loan is cancelled, equipment status unchanged.
- **FR-016**: Employees with `facility.loans.write` permission MUST be able to record an equipment return, with optional condition notes. On return: equipment status transitions to `Active`, loan status becomes `Returned`, and `EquipmentStatusChangedEvent` is published.
- **FR-017**: Loans whose `ExpectedReturnDate` has passed and whose `LoanStatus` is still `Active` MUST be surfaced as `Overdue` (either via background flag-update or at-query-time computation).

**Maintenance**

- **FR-018**: Employees with `facility.maintenance.write` permission MUST be able to append a maintenance log entry to any equipment record. Required fields: `Type`, `OccurredAt`, `Description`. Optional: `VendorName`, `CostTHB`, `NextServiceDueDate`.
- **FR-019**: When a maintenance log entry includes `NextServiceDueDate`, the equipment's `NextServiceDueDate` field MUST be updated to that date.
- **FR-020**: Maintenance log entries are immutable once created — no edit or delete is permitted.

**CNC Attachments**

- **FR-021**: Employees with `facility.attachments.write` permission MUST be able to add attachment records to CNC machine equipment only. Required: `Name`, `AttachmentType`. Optional: `SerialNumber`, `ConditionNotes`.
- **FR-022**: Employees with `facility.attachments.write` permission MUST be able to mark an attachment as retired (`IsActive = false`) with optional condition notes.
- **FR-023**: The system MUST reject attachment creation on non-CNC equipment with HTTP 422.

**Access Control**

- **FR-024**: All endpoints MUST use `[RequirePermission]` with GCP-style permission strings. Permissions are:
  - `facility.equipments.read` — all employee roles + PricingService + JobService
  - `facility.equipments.write` — Admin, Manager
  - `facility.equipments.manage` — Admin only
  - `facility.loans.read` — Admin, Manager
  - `facility.loans.write` — Admin, Manager
  - `facility.loans.approve` — Admin, Manager
  - `facility.maintenance.read` — all employee roles
  - `facility.maintenance.write` — Admin, Manager, Technician
  - `facility.attachments.write` — Admin, Manager

**Concurrency**

- **FR-025**: The system MUST use PostgreSQL `xmin` optimistic concurrency on the `Equipment` entity. Concurrent updates MUST result in HTTP 409 for the second writer.

---

## Key Entities

- **Equipment** (abstract base): All company-owned assets. Subtypes: `ManufacturingEquipment` (abstract, adds pricing fields) → `FdmPrinterEquipment`, `SlaPrinterEquipment`, `CncMachineEquipment`, `Scanner3DEquipment`, `InjectionMoldingEquipment`; `GeneralEquipment` (abstract, no pricing fields) → `OfficeEquipmentItem`, `MeasuringEquipmentItem`, `ITEquipmentItem`, `HandToolItem`, `OtherEquipmentItem`.
- **EquipmentNote**: Append-only, immutable timestamped note attached to an equipment record.
- **EquipmentLoan**: Tracks who has a piece of equipment (employee or customer), loan dates, approval, and return. Customer loans require manager approval and trigger PDF generation.
- **EquipmentMaintenanceLog**: Immutable per-event maintenance history entry with type, description, vendor, cost, and next service date.
- **EquipmentAttachment**: Durable fixture or accessory attached to a CNC machine (vise, tool holder, fixture). Tracks active/retired state and condition.

---

## Clarifications

### Session 2026-02-25 (Revised Scope)

- All company assets (not just manufacturing machines) are managed by FacilityService.
- Lending scope includes both employees (informal, no approval) and customers (approval required, PDF document via PdfService event).
- `Lost` is a valid status; `Lost → Active` is permitted (equipment found).
- `OnLoan` excludes equipment from all pricing/job queries (same as UnderMaintenance).
- Notes are append-only child records, not a single overwritable field.
- Material-specific process parameters (FDM layer times, SLA exposure/lift times) belong to MaterialService — removed from FacilityService spec tables.
- CNC `MaterialRemovalRate` removed — it is material- and tooling-dependent, not a machine property. CNC capability is described by spindle speed, power, travel envelope, tool interface, and axes.
- Cutting tools and inserts are consumables → InventoryService. Only durable fixtures (vises, tool holders) are tracked in FacilityService.
- Database pattern: Table-per-Type (TPT) — `equipments` shared table + one spec table per manufacturing subtype. General equipment has no spec table → zero nullable manufacturing columns on general equipment rows.
- `Brand`, `ModelName`, and `PurchasePriceTHB` added to base Equipment entity.
- Maintenance log entries are immutable (append-only) for audit integrity.

---

## Assumptions

- All monetary values are stored in Thai Baht (THB).
- All timestamps are UTC.
- `AssetCode` is immutable after creation. `Category` is immutable after creation.
- Initial status on registration is always `Active`.
- Decommissioned is a terminal state — no transitions out.
- `isOutsourced` is derived at query time (no active machine for requested category) and is never stored.
- Equipment names are unique across all categories to prevent duplicate asset registration.
- Maintenance log entries and notes are immutable for audit integrity.
- Customer loan PDF rendering is fully delegated to `Maliev.PdfService` via event — FacilityService does not generate PDFs.
- CNC attachments are only meaningful for `CncMachineEquipment` — other categories reject attachment creation.
- The `GET /active` query endpoint serves both PricingService and JobService with the same response shape.

---

## Success Criteria

- **SC-001**: An employee can register any equipment type and have it appear in the asset list in under 30 seconds.
- **SC-002**: PricingService can retrieve all active machines for a given category and receive a complete response in under 500 ms at p95.
- **SC-003**: After a status change, `EquipmentStatusChangedEvent` is delivered to RabbitMQ within 5 seconds via the MassTransit outbox.
- **SC-004**: Concurrent updates to the same equipment record return HTTP 409 to the second writer in 100% of cases.
- **SC-005**: A query for a category with no active machines returns HTTP 200 with `isOutsourced: true` in 100% of cases — never an error.
- **SC-006**: Hard deletion of a machine with job history is rejected in 100% of cases with a message directing the employee to use Decommissioned status.
- **SC-007**: All equipment records (and their child records) are retained after decommissioning — no data is lost as a result of a status change.
- **SC-008**: `LoanDocumentRequestedEvent` is published within 5 seconds of customer loan approval, with complete structured loan data for PdfService to render.
- **SC-009**: Equipment with `OnLoan` or `Lost` status is excluded from active-equipment queries in 100% of cases.
- **SC-010**: Notes and maintenance log entries cannot be modified or deleted — immutability enforced at the API layer with 405 Method Not Allowed.
