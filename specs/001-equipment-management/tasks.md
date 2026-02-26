# Tasks: Facility & Equipment Management

**Input**: Design documents from `/specs/001-equipment-management/`
**Prerequisites**: plan.md ✅ spec.md ✅ research.md ✅ data-model.md ✅ contracts/ ✅ quickstart.md ✅
**Branch**: `001-equipment-management`
**Tests**: Included — spec.md mandates ≥ 80% coverage and specifies integration test scenarios.

**Organization**: Tasks are grouped by user story (P1–P12) to enable independent implementation and testing.

---

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies on incomplete tasks)
- **[Story]**: Which user story this task belongs to (P1–P12)

---

## Phase 1: Setup (Project Initialization)

**Purpose**: Bootstrap the solution, projects, shared config files, and GitHub scaffolding. All setup tasks can proceed in parallel after T001.

- [X] T001 Create solution file `Maliev.FacilityService.slnx` at repo root
- [X] T002 [P] Create `Maliev.FacilityService.Api` web API project in `Maliev.FacilityService.Api/`
- [X] T003 [P] Create `Maliev.FacilityService.Application` class library in `Maliev.FacilityService.Application/`
- [X] T004 [P] Create `Maliev.FacilityService.Domain` class library in `Maliev.FacilityService.Domain/`
- [X] T005 [P] Create `Maliev.FacilityService.Infrastructure` class library in `Maliev.FacilityService.Infrastructure/`
- [X] T006 [P] Create `Maliev.FacilityService.Tests` xUnit project in `Maliev.FacilityService.Tests/`
- [X] T007 Add all five projects to `Maliev.FacilityService.slnx`
- [X] T008 [P] Delete all default boilerplate files (`Class1.cs` in all libs; `WeatherForecast.cs`, any example controllers in Api; `UnitTest1.cs` in Tests)
- [X] T009 Add project-to-project references per Clean Architecture rules: Application→Domain; Infrastructure→Application+Domain; Api→Application+Infrastructure; Tests→all four
- [X] T010 [P] Create `Directory.Build.props` at repo root with `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` and `<Nullable>enable</Nullable>`
- [X] T011 [P] Add NuGet packages to `Maliev.FacilityService.Api`: `Maliev.Aspire.ServiceDefaults`, `Scalar.AspNetCore`
- [X] T012 [P] Add NuGet packages to `Maliev.FacilityService.Infrastructure`: `Npgsql.EntityFrameworkCore.PostgreSQL`, `MassTransit.RabbitMQ`, `MassTransit.EntityFrameworkCore`
- [X] T013 [P] Add NuGet packages to `Maliev.FacilityService.Tests`: `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `Moq`, `coverlet.collector`, `Testcontainers.PostgreSql`, `MassTransit.TestFramework`
- [X] T014 [P] Create `Maliev.FacilityService.Api/Dockerfile` using optimized multi-stage build with `app` user per constitution spec
- [X] T015 [P] Create `.github/CODEOWNERS` file
- [X] T016 [P] Create `.github/dependabot.yml` with .NET and NuGet configuration
- [X] T017 [P] Create `.github/workflows/pr-validation.yml` CI workflow for pull request builds

**Checkpoint**: Solution builds cleanly (`dotnet build Maliev.FacilityService.slnx`) with zero warnings.

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core domain types, database context, permission system, and API infrastructure that ALL user stories depend on. No user story work can begin until this phase is complete.

**⚠️ CRITICAL**: No user story work can begin until this phase is complete.

### Enums
- [X] T018 [P] Create `EquipmentCategory` enum in `Maliev.FacilityService.Domain/Enums/EquipmentCategory.cs` with all 10 values (FdmPrinter, SlaPrinter, CncMachine, Scanner3D, InjectionMolding, OfficeEquipment, MeasuringEquipment, ITEquipment, HandTool, Other) — stored as string in DB
- [X] T019 [P] Create `EquipmentStatus` enum in `Maliev.FacilityService.Domain/Enums/EquipmentStatus.cs` with all 5 values (Active, UnderMaintenance, OnLoan, Lost, Decommissioned) — stored as string in DB
- [X] T020 [P] Create domain-specific enums: `SlaLightSourceType`, `CncToolInterface`, `Scanner3DTechnology`, `MaintenanceType`, `LoanBorrowerType`, `LoanStatus`, `AttachmentType`

### Exceptions
- [X] T021 [P] Create `InvalidStatusTransitionException` in `Maliev.FacilityService.Domain/Exceptions/InvalidStatusTransitionException.cs` (thrown on illegal status transitions → HTTP 422)
- [X] T022 [P] Create `EquipmentHasJobHistoryException` in `Maliev.FacilityService.Domain/Exceptions/EquipmentHasJobHistoryException.cs` (thrown when hard delete is blocked → HTTP 409)
- [X] T023 [P] Create `EquipmentNotFoundException` in `Maliev.FacilityService.Domain/Exceptions/EquipmentNotFoundException.cs` (thrown when equipment ID not found → HTTP 404)
- [X] T024 [P] Create `AttachmentNotAllowedException` in `Maliev.FacilityService.Domain/Exceptions/AttachmentNotAllowedException.cs` (thrown when attachment is created on non-CNC equipment → HTTP 422)
- [X] T025 [P] Create `LoanNotAllowedException` in `Maliev.FacilityService.Domain/Exceptions/LoanNotAllowedException.cs` (thrown when loan cannot be created → HTTP 409)

### Authorization
- [X] T026 [P] Create `FacilityPermissions` static class in `Maliev.FacilityService.Domain/Authorization/FacilityPermissions.cs` with all permission string constants and `AllWithDescriptions` dictionary
- [X] T027 [P] Create `FacilityPredefinedRoles` static class in `Maliev.FacilityService.Domain/Authorization/FacilityPredefinedRoles.cs` mapping Admin/Manager/Viewer/Technician roles to permission sets

### Domain Entities (TPT)
- [X] T028 [P] Create `Equipment` abstract base entity in `Maliev.FacilityService.Domain/Entities/Equipment.cs` with all base fields: Id, AssetCode, Brand, ModelName, Name, ManufacturerSerialNumber, Category, SubCategory, Status, PurchaseDate, PurchasePriceTHB, WarrantyExpiryDate, NextServiceDueDate, CreatedAt, UpdatedAt, xmin
- [X] T029 [P] Create `ManufacturingEquipment` abstract entity in `Maliev.FacilityService.Domain/Entities/ManufacturingEquipment.cs` with HourlyRateTHB, SetupFeeTHB, ExtendedProperties
- [X] T030 [P] Create concrete manufacturing entities: `FdmPrinterEquipment`, `SlaPrinterEquipment`, `CncMachineEquipment`, `Scanner3DEquipment`, `InjectionMoldingEquipment`
- [X] T031 [P] Create `GeneralEquipment` abstract entity in `Maliev.FacilityService.Domain/Entities/GeneralEquipment.cs`
- [X] T032 [P] Create concrete general entities: `OfficeEquipmentItem`, `MeasuringEquipmentItem`, `ITEquipmentItem`, `HandToolItem`, `OtherEquipmentItem`
- [X] T033 [P] Create `EquipmentNote` entity in `Maliev.FacilityService.Domain/Entities/EquipmentNote.cs`
- [X] T034 [P] Create `EquipmentLoan` entity in `Maliev.FacilityService.Domain/Entities/EquipmentLoan.cs`
- [X] T035 [P] Create `EquipmentMaintenanceLog` entity in `Maliev.FacilityService.Domain/Entities/EquipmentMaintenanceLog.cs`
- [X] T036 [P] Create `EquipmentAttachment` entity in `Maliev.FacilityService.Domain/Entities/EquipmentAttachment.cs`

### Application Interfaces
- [X] T037 [P] Create `IEquipmentRepository` interface in `Maliev.FacilityService.Application/Interfaces/IEquipmentRepository.cs` with all CRUD + query methods
- [X] T038 [P] Create `ILoanRepository` interface in `Maliev.FacilityService.Application/Interfaces/ILoanRepository.cs`
- [X] T039 [P] Create `IMaintenanceLogRepository` interface in `Maliev.FacilityService.Application/Interfaces/IMaintenanceLogRepository.cs`
- [X] T040 [P] Create `IAttachmentRepository` interface in `Maliev.FacilityService.Application/Interfaces/IAttachmentRepository.cs`
- [X] T041 [P] Create `IAssetCodeGenerator` interface in `Maliev.FacilityService.Application/Interfaces/IAssetCodeGenerator.cs`
- [X] T042 [P] Create `IJobServiceClient` interface in `Maliev.FacilityService.Application/Interfaces/IJobServiceClient.cs` with `HasHistoricalJobsAsync`

### Infrastructure
- [X] T043 [P] Create `FacilityDbContext` in `Maliev.FacilityService.Infrastructure/Data/FacilityDbContext.cs` with all DbSets and OnModelCreating calling outbox entities
- [X] T044 [P] Create entity configurations in `Maliev.FacilityService.Infrastructure/Data/Configurations/` for all entities
- [X] T045 [P] Create `EquipmentRepository` in `Maliev.FacilityService.Infrastructure/Data/Repositories/EquipmentRepository.cs` implementing `IEquipmentRepository`
- [X] T046 [P] Create `LoanRepository`, `MaintenanceLogRepository`, `AttachmentRepository` implementing their respective interfaces
- [X] T047 [P] Create `AssetCodeGenerator` in `Maliev.FacilityService.Infrastructure/Services/AssetCodeGenerator.cs` implementing `IAssetCodeGenerator`
- [X] T048 [P] Create `JobServiceClient` in `Maliev.FacilityService.Infrastructure/ExternalClients/JobServiceClient.cs`
- [X] T049 [P] Create `DependencyInjection.cs` in `Maliev.FacilityService.Infrastructure/DependencyInjection.cs`

### Middleware & Program.cs
- [X] T050 [P] Create `ConcurrencyExceptionMiddleware` in `Maliev.FacilityService.Api/Middleware/ConcurrencyExceptionMiddleware.cs`
- [X] T051 [P] Create `NotFoundExceptionMiddleware` in `Maliev.FacilityService.Api/Middleware/NotFoundExceptionMiddleware.cs`
- [X] T052 [P] Wire up `Maliev.FacilityService.Api/Program.cs`: call `builder.AddServiceDefaults()`, register all services, add middleware, configure Scalar at `/facility/scalar`, set launchSettings

### Messaging Contracts
- [X] T053 [P] Add `EquipmentStatusChangedEvent` JSON schema to `Maliev.MessagingContracts`
- [X] T054 [P] Add `LoanDocumentRequestedEvent` JSON schema to `Maliev.MessagingContracts`
- [X] T055 [P] Run `./scripts/build.ps1` in MessagingContracts to generate C# record types
- [X] T056 [P] Add `Maliev.MessagingContracts` package reference to Infrastructure project

### Migrations & Test Infrastructure
- [X] T057 Run `dotnet ef migrations add InitialCreate` and verify generated migration includes all tables, indexes, and outbox tables
- [X] T058 [P] Create `PostgresFixture.cs` shared Testcontainers PostgreSQL container fixture

### IAM Registration
- [X] T059 Create `FacilityIAMRegistrationService` in `Maliev.FacilityService.Api/Services/FacilityIAMRegistrationService.cs` implementing `IAMRegistrationService`

**Checkpoint**: `dotnet build Maliev.FacilityService.slnx` passes with zero warnings. Domain entities, DbContext, repositories, middleware, and messaging contract C# records are all in place.

---

## Phase 3: User Story 1 — Register Equipment (P1)

**Goal**: An employee can register any equipment category with all required fields. System auto-generates unique asset code.

**Independent Test**: `POST /facility/v1/equipments` with valid FDM payload → 201 Created with `assetCode: MAL-FDM-0001`.

### Implementation

- [X] T060 [P] [P1] Create `RegisterEquipmentCommand` record with all registration fields
- [X] T061 [P1] Create `RegisterEquipmentCommandHandler`: validate, generate asset code, create entity, persist, return mapped DTO
- [X] T062 [P1] Implement `POST /facility/v1/equipments` action in `EquipmentsController.cs`

### Unit Tests
- [X] T063 [P] [P1] Create `RegisterEquipmentCommandTests`

### Integration Tests
- [X] T064 [P] [P1] Create integration tests for equipment registration

**Checkpoint**: Equipment registration works for all 10 categories with correct asset code generation.

---

## Phase 4: User Story 2 — Query Active Manufacturing Equipment (P2)

**Goal**: PricingService and JobService can query all Active manufacturing equipment by category.

**Independent Test**: Seed Active + UnderMaintenance FDM printers → query → only Active returned with `isOutsourced: false`.

### Implementation

- [X] T065 [P] [P2] Create `GetActiveEquipmentsByCategoryQuery` record
- [X] T066 [P2] Create `GetActiveEquipmentsByCategoryQueryHandler`: query by category + Active status, return envelope with isOutsourced
- [X] T067 [P2] Implement `GET /facility/v1/equipments/active` action

### Unit Tests
- [X] T068 [P] [P2] Create `GetActiveEquipmentsByCategoryQueryTests`

### Integration Tests
- [X] T069 [P] [P2] Create integration tests for active equipment query

**Checkpoint**: Query endpoint returns correct equipment with isOutsourced flag.

---

## Phase 5: User Story 3 — Update Equipment Status (P3)

**Goal**: An employee can change equipment status according to permitted transitions. Event published on change.

**Independent Test**: Active → UnderMaintenance → Active → Decommissioned → attempt restore → 422.

### Implementation

- [X] T070 [P] [P3] Add `TransitionTo(EquipmentStatus)` method to Equipment entity with transition validation
- [X] T071 [P] [P3] Create `ChangeEquipmentStatusCommand` record
- [X] T072 [P3] Create `ChangeEquipmentStatusCommandHandler`: load equipment, validate transition, persist, publish `EquipmentStatusChangedEvent`
- [X] T073 [P3] Implement `PATCH /facility/v1/equipments/{id}/status` action

### Unit Tests
- [X] T074 [P] [P3] Create `ChangeEquipmentStatusCommandTests`

### Integration Tests
- [X] T075 [P] [P3] Create integration tests for status changes + event publication

**Checkpoint**: All status transitions work, event published, invalid transitions rejected with 422.

---

## Phase 6: User Story 4 — List & Filter Equipment (P4)

**Goal**: An employee can browse all equipment with filters.

**Independent Test**: Seed mixed equipment → filter by category → filter by status → combined filter.

### Implementation

- [X] T076 [P] [P4] Create `ListEquipmentsQuery` record with filters and pagination
- [X] T077 [P4] Create `ListEquipmentsQueryHandler`
- [X] T078 [P4] Implement `GET /facility/v1/equipments` action

### Integration Tests
- [X] T079 [P] [P4] Create integration tests for list + filter + pagination

**Checkpoint**: Filtering and pagination work correctly.

---

## Phase 7: User Story 5 — Update Equipment Details (P5)

**Goal**: An employee can update equipment details with optimistic concurrency.

**Independent Test**: Update name → 200 → re-submit with old rowVersion → 409.

### Implementation

- [X] T080 [P] [P5] Create `UpdateEquipmentCommand` record
- [X] T081 [P5] Create `UpdateEquipmentCommandHandler`
- [X] T082 [P5] Implement `PUT /facility/v1/equipments/{id}` action

### Unit Tests
- [X] T083 [P] [P5] Create `UpdateEquipmentCommandTests`

### Integration Tests
- [X] T084 [P] [P5] Create integration tests for updates

**Checkpoint**: Updates work with 409 on concurrent modification.

---

## Phase 8: User Story 6 — Equipment Notes (P6)

**Goal**: Employees can add timestamped notes to equipment. Notes are immutable.

**Independent Test**: Add two notes → list → both appear in order.

### Implementation

- [X] T085 [P] [P6] Create `AddEquipmentNoteCommand` record
- [X] T086 [P6] Create `AddEquipmentNoteCommandHandler`
- [X] T087 [P6] Create `GetEquipmentNotesQuery` and handler
- [X] T088 [P6] Implement `POST /facility/v1/equipments/{id}/notes` and `GET /facility/v1/equipments/{id}/notes` actions

### Integration Tests
- [X] T089 [P] [P6] Create integration tests for notes

**Checkpoint**: Notes can be added and listed. Edit/delete rejected.

---

## Phase 9: User Story 7 — Employee Loans (P7)

**Goal**: Manager can assign equipment to an employee. Equipment status changes to OnLoan immediately.

**Independent Test**: Create employee loan → status OnLoan → return → status Active.

### Implementation

- [X] T090 [P] [P7] Create `CreateLoanCommand` record
- [X] T091 [P7] Create `CreateLoanCommandHandler` (employee path: immediate OnLoan)
- [X] T092 [P7] Create `ReturnLoanCommand` record
- [X] T093 [P7] Create `ReturnLoanCommandHandler`
- [X] T094 [P7] Implement loan endpoints in `LoansController.cs`

### Integration Tests
- [X] T095 [P] [P7] Create integration tests for employee loans

**Checkpoint**: Employee loans work, equipment OnLoan.

---

## Phase 10: User Story 8 — Customer Loans (P8)

**Goal**: Customer loans require approval. On approval, equipment OnLoan + event published for PDF.

**Independent Test**: Create customer loan → PendingApproval → approve → OnLoan + event published.

### Implementation

- [X] T096 [P] [P8] Update `CreateLoanCommandHandler` to handle Customer loans (create as PendingApproval)
- [X] T097 [P] [P8] Create `ApproveLoanCommand` record
- [X] T098 [P8] Create `ApproveLoanCommandHandler`: approve loan, set equipment OnLoan, publish `LoanDocumentRequestedEvent`
- [X] T099 [P8] Create `RejectLoanCommand` handler
- [X] T100 [P8] Implement `PATCH /loans/{loanId}/approve`, `/reject`, `/return` actions

### Unit Tests
- [X] T101 [P] [P8] Create loan command tests

### Integration Tests
- [X] T102 [P] [P8] Create integration tests for customer loans + event publication

**Checkpoint**: Customer loan workflow works, PDF event published.

---

## Phase 11: User Story 9 — Maintenance Logs (P9)

**Goal**: Employees can log maintenance events. Next service due date denormalized to equipment.

**Independent Test**: Log maintenance with next service date → equipment.NextServiceDueDate updated.

### Implementation

- [X] T103 [P] [P9] Create `AddMaintenanceLogCommand` record
- [X] T104 [P9] Create `AddMaintenanceLogCommandHandler`: validate, persist, update NextServiceDueDate
- [X] T105 [P9] Create `GetMaintenanceLogsQuery` handler
- [X] T106 [P9] Implement `POST /facility/v1/equipments/{id}/maintenance` and `GET` actions

### Integration Tests
- [X] T107 [P] [P9] Create integration tests for maintenance logs

**Checkpoint**: Maintenance logs created, NextServiceDueDate denormalized.

---

## Phase 12: User Story 10 — CNC Attachments (P10)

**Goal**: Managers can add/manage CNC machine attachments (vises, fixtures).

**Independent Test**: Add attachment to CNC → list → appears. Add to non-CNC → 422.

### Implementation

- [X] T108 [P] [P10] Create `AddAttachmentCommand` record
- [X] T109 [P10] Create `AddAttachmentCommandHandler`: validate equipment is CNC, persist
- [X] T110 [P10] Create `UpdateAttachmentCommand` handler (mark retired)
- [X] T111 [P10] Implement attachment endpoints in `AttachmentsController.cs`

### Integration Tests
- [X] T112 [P] [P10] Create integration tests for attachments

**Checkpoint**: Attachments only allowed on CNC equipment.

---

## Phase 13: User Story 11 — Hard Delete Equipment (P11)

**Goal**: Admin can permanently delete equipment if no job history and no active loans.

**Independent Test**: Delete with no history → 204. Delete with history → 409.

### Implementation

- [X] T113 [P] [P11] Create `DeleteEquipmentCommand` record
- [X] T114 [P11] Create `DeleteEquipmentCommandHandler`: check job history, check active loan, cascade delete
- [X] T115 [P11] Implement `DELETE /facility/v1/equipments/{id}` action

### Unit Tests
- [X] T116 [P] [P11] Create `DeleteEquipmentCommandTests`

**Checkpoint**: Hard delete blocked when history exists.

---

## Phase 14: User Story 12 — Query Available Machines for Job Assignment (P12)

**Goal**: JobService queries available machines by category. Uses same endpoint as P2.

*Note: The `GET /active` endpoint was already implemented in Phase 4 (P2) — it serves both PricingService and JobService.*

### Implementation

- [X] T117 [P] [P12] Implement `GET /facility/v1/equipments/{id}` detail endpoint
- [X] T118 [P] [P12] Create `GetEquipmentByIdQuery` handler

### Integration Tests
- [X] T119 [P] [P12] Create integration tests for detail endpoint

**Checkpoint**: All seven user stories are independently functional.

---

## Phase 15: Polish & Cross-Cutting Concerns

**Purpose**: Quality hardening, XML documentation, Aspire registration, performance acceptance criteria verification.

- [X] T120 [P] Add XML documentation comments to all public methods, properties, and classes
- [X] T121 [P] Create `FacilityPermissionsTests`: verify every permission is assigned to at least one role
- [X] T122 Register `Maliev.FacilityService.Api` in `Maliev.Aspire` AppHost
- [X] T123 [P] Configure `appsettings.json` connection string key as `"FacilityDbContext"`
- [X] T124 Run full test suite and verify ≥ 80% code coverage
- [X] T125 [P] Validate no secrets are committed
- [X] T126 [P] Verify SC-001, SC-002, SC-003 are met (response times)
- [X] T127 Perform end-to-end smoke test per `quickstart.md`

**Checkpoint**: All tests pass, coverage ≥ 80%, zero warnings, no secrets, Aspire integration works.

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — start immediately
- **Foundational (Phase 2)**: Depends on Phase 1 completion — **BLOCKS all user stories**
- **User Stories (Phases 3–14)**: All depend on Phase 2 completion
- **Polish (Phase 15)**: Depends on all user stories complete

### User Story Dependencies

- **P1 — Register**: Starts after Phase 2. No dependency.
- **P2 — Query Active**: Starts after Phase 2. Uses same endpoint for P12.
- **P3 — Update Status**: Starts after Phase 2.
- **P4 — List & Filter**: Starts after Phase 2.
- **P5 — Update Details**: Starts after Phase 2.
- **P6 — Notes**: Starts after Phase 2.
- **P7 — Employee Loans**: Starts after Phase 2.
- **P8 — Customer Loans**: Starts after Phase 2. Depends on P7.
- **P9 — Maintenance Logs**: Starts after Phase 2.
- **P10 — CNC Attachments**: Starts after Phase 2.
- **P11 — Hard Delete**: Starts after Phase 2.
- **P12 — Query By ID**: Starts after Phase 2.

### Parallel Opportunities

- All `[P]` tasks within each phase can run simultaneously
- After Phase 2: P1, P2, P4, P5, P6, P7, P9, P10 can begin in parallel (independent files)
- P8 depends on P7 (loan workflow)
- P11 (delete) is independent
- P12 (detail endpoint) is mostly additive

---

## Implementation Strategy

### MVP (P1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational
3. Complete Phase 3: P1 — Register Equipment
4. **STOP and VALIDATE**: Registration + asset code generation works
5. Demo to stakeholders

### Incremental Delivery

1. Phase 1 + Phase 2 → Foundation ready
2. Phase 3 (P1) → Equipment registration MVP
3. Phase 4 (P2) → PricingService can query machines
4. Phase 5 (P3) → Status management + events
5. Phase 6 (P4) → Employee list/filter view
6. Phase 7 (P5) → Updates
7. Phase 8 (P6) → Notes
8. Phase 9 (P7) → Employee loans
9. Phase 10 (P8) → Customer loans + PDF event
10. Phase 11 (P9) → Maintenance logs
11. Phase 12 (P10) → CNC attachments
12. Phase 13 (P11) → Delete
13. Phase 14 (P12) → Detail endpoint
14. Phase 15 → Polish, coverage, Aspire

### Parallel Team Strategy

With two developers after Phase 2:
- **Dev A**: P1 (register) → P5 (update) → P6 (notes) → P11 (delete)
- **Dev B**: P2 (query active) → P3 (status + events) → P4 (list/filter) → P7/P8 (loans) → P9 (maintenance) → P10 (attachments)

---

## Notes

- `[P]` tasks touch different files and have no incomplete dependencies — safe to parallelize
- `EquipmentsController.cs`, `LoansController.cs`, `MaintenanceController.cs`, `AttachmentsController.cs` are shared across multiple user stories — serialize writes to these files or branch per user story to avoid conflicts
- The `GET /active` endpoint serves both PricingService and JobService — implement once in P2, reference in P12
- Never commit `*.local.json`, `.env`, or any file containing connection strings
- Commit after each checkpoint to preserve incremental progress
- Run `dotnet build` after every task to catch regressions early (TreatWarningsAsErrors=true)

(End of file)
