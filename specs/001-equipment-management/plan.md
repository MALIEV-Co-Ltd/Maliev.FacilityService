# Implementation Plan: Facility & Equipment Management

**Branch**: `001-equipment-management` | **Date**: 2026-02-25 (Revised) | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-equipment-management/spec.md`

---

## Summary

Implement `Maliev.FacilityService` as a new Clean Architecture microservice that manages **all company-owned physical assets** — not just manufacturing machines. The service handles the full asset lifecycle: registration with auto-generated asset codes, lending to employees and customers (with approval workflows), maintenance logging, CNC attachment tracking, notes, and eventual decommissioning.

**Key capabilities:**
- Multi-category equipment registration (FDM/SLA/CNC/Scanner/InjectionMolding + general assets: Office/IT/Measuring/HandTools/Other)
- TPT (Table-Per-Type) database schema for clean data — zero nullable manufacturing columns on general equipment rows
- Asset code auto-generation (`MAL-{PREFIX}-{SEQ}`) with per-category sequences
- Full lending lifecycle: employee loans (immediate), customer loans (approval + PDF document via event)
- Maintenance log with per-event entries, vendor tracking, and next-service-date denormalization
- CNC attachments (vises, fixtures, tool holders)
- Equipment notes (append-only, immutable)
- Status transitions: Active ↔ UnderMaintenance ↔ OnLoan ↔ Lost; any → Decommissioned (terminal); Lost → Active (recovery)
- Optimistic concurrency via PostgreSQL xmin
- MassTransit outbox for `EquipmentStatusChangedEvent` and `LoanDocumentRequestedEvent`

---

## Technical Context

**Language/Version**: C# / .NET 10
**Primary Dependencies**: ASP.NET Core 10, EF Core 10 + Npgsql, MassTransit 8.x, MassTransit.EntityFrameworkCore, Maliev.Aspire.ServiceDefaults
**Storage**: PostgreSQL (dedicated `facility-app-db`), TPT schema with typed spec tables per manufacturing subtype, `xmin` row-version for optimistic concurrency, MassTransit outbox tables
**Testing**: xUnit 2.x, Moq, Testcontainers.PostgreSql, MassTransit.TestFramework; coverage target ≥ 80%
**Target Platform**: Linux container (GKE via ArgoCD), local Aspire orchestration
**Project Type**: Web service (internal microservice)
**Performance Goals**: Active equipment query response < 500ms p95 (SC-002); status change event delivery < 5s (SC-003)
**Constraints**: No AutoMapper, FluentValidation, Swashbuckle/Swagger, FluentAssertions, Serilog, InMemoryDatabase; TreatWarningsAsErrors=true; mandatory XML docs; all secrets via ESO
**Scale/Scope**: Moderate — registration/updates ~10s/day; lending/maintenance ~20s/day; PricingService and JobService queries on every quote/job

---

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

| Rule | Check | Status |
|---|---|---|
| I. Target Framework .NET 10 | All projects target `net10.0` | ✅ |
| II. `AddServiceDefaults()` | Called in `Api/Program.cs`; no manual OTel/health/Polly | ✅ |
| II. Aspire integration | Registered in `Maliev.Aspire` AppHost | ✅ |
| III. `.slnx` solution format | `Maliev.FacilityService.slnx` | ✅ |
| III. No boilerplate | `Class1.cs`, `WeatherForecast.cs`, `UnitTest1.cs` deleted | ✅ |
| IV. Route prefix + versioning | All routes under `/facility/v1/` | ✅ |
| IV. Scalar launchSettings | `launchSettings.json` opens `/facility/scalar` | ✅ |
| V. Scalar, no Swagger/Swashbuckle | Scalar only; Swashbuckle banned | ✅ |
| V. XML documentation | Mandatory on all public members | ✅ |
| VI. Centralized messaging contracts | `EquipmentStatusChangedEvent` + `LoanDocumentRequestedEvent` defined in `Maliev.MessagingContracts` | ✅ |
| VII. GCP-style `[RequirePermission]` | All endpoints use `[RequirePermission]`; no plain `[Authorize]` | ✅ |
| VII. Plural resource format | `facility.equipments.*`, `facility.loans.*`, `facility.maintenance.*`, `facility.attachments.*` | ✅ |
| VIII. Zero secrets | No hardcoded connection strings or credentials | ✅ |
| X. TreatWarningsAsErrors | Inherited from `Directory.Build.props` | ✅ |
| X. No banned libraries | No AutoMapper, FluentValidation, FluentAssertions, Serilog | ✅ |
| XI. CODEOWNERS + Dependabot + workflows | Required at `.github/` | ✅ |
| XII. ≥ 80% test coverage | Enforced via coverlet | ✅ |
| XII. Testcontainers | PostgreSQL integration tests via Testcontainers | ✅ |
| XIII. Dockerfile in Api project | Multi-stage, `app` user | ✅ |

**Gate result: PASS** — No violations. Proceed to Phase 1.

---

## Project Structure

### Documentation (this feature)

```
specs/001-equipment-management/
├── plan.md              # This file (/speckit.plan output)
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output (TPT schema)
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output
│   ├── api-endpoints.md
│   └── messaging-contracts.md
├── checklists/
│   └── requirements.md
└── tasks.md             # Phase 2 output (/speckit.tasks)
```

### Source Code (repository root)

```
Maliev.FacilityService.slnx

Maliev.FacilityService.Api/
├── Controllers/
│   ├── EquipmentsController.cs
│   ├── LoansController.cs
│   ├── MaintenanceController.cs
│   └── AttachmentsController.cs
├── Services/
│   └── FacilityIAMRegistrationService.cs
├── Middleware/
│   ├── ConcurrencyExceptionMiddleware.cs
│   └── NotFoundExceptionMiddleware.cs
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── launchSettings.json
└── Dockerfile

Maliev.FacilityService.Application/
├── UseCases/
│   ├── Commands/
│   │   ├── RegisterEquipment/
│   │   ├── UpdateEquipment/
│   │   ├── ChangeEquipmentStatus/
│   │   ├── DeleteEquipment/
│   │   ├── AddEquipmentNote/
│   │   ├── CreateLoan/
│   │   ├── ApproveLoan/
│   │   ├── ReturnLoan/
│   │   ├── AddMaintenanceLog/
│   │   └── ManageAttachment/
│   └── Queries/
│       ├── GetEquipmentById/
│       ├── ListEquipments/
│       ├── GetActiveEquipmentsByCategory/
│       └── GetEquipmentNotes/
├── DTOs/
│   ├── EquipmentDto.cs
│   ├── EquipmentSummaryDto.cs
│   ├── ActiveEquipmentDto.cs
│   ├── LoanDto.cs
│   ├── MaintenanceLogDto.cs
│   └── AttachmentDto.cs
├── Interfaces/
│   ├── IEquipmentRepository.cs
│   ├── ILoanRepository.cs
│   ├── IMaintenanceLogRepository.cs
│   ├── IAttachmentRepository.cs
│   ├── IAssetCodeGenerator.cs
│   └── IJobServiceClient.cs
└── Validators/
    └── EquipmentSpecValidator.cs

Maliev.FacilityService.Domain/
├── Entities/
│   ├── Equipment.cs (abstract base)
│   ├── ManufacturingEquipment.cs (abstract)
│   │   ├── FdmPrinterEquipment.cs
│   │   ├── SlaPrinterEquipment.cs
│   │   ├── CncMachineEquipment.cs
│   │   ├── Scanner3DEquipment.cs
│   │   └── InjectionMoldingEquipment.cs
│   ├── GeneralEquipment.cs (abstract)
│   │   ├── OfficeEquipmentItem.cs
│   │   ├── MeasuringEquipmentItem.cs
│   │   ├── ITEquipmentItem.cs
│   │   ├── HandToolItem.cs
│   │   └── OtherEquipmentItem.cs
│   ├── EquipmentNote.cs
│   ├── EquipmentLoan.cs
│   ├── EquipmentMaintenanceLog.cs
│   └── EquipmentAttachment.cs
├── Enums/
│   ├── EquipmentCategory.cs
│   ├── EquipmentStatus.cs
│   ├── SlaLightSourceType.cs
│   ├── CncToolInterface.cs
│   ├── Scanner3DTechnology.cs
│   ├── MaintenanceType.cs
│   ├── LoanBorrowerType.cs
│   ├── LoanStatus.cs
│   └── AttachmentType.cs
├── Authorization/
│   ├── FacilityPermissions.cs
│   └── FacilityPredefinedRoles.cs
└── Exceptions/
    ├── InvalidStatusTransitionException.cs
    ├── EquipmentHasJobHistoryException.cs
    ├── EquipmentNotFoundException.cs
    ├── AttachmentNotAllowedException.cs
    └── LoanNotAllowedException.cs

Maliev.FacilityService.Infrastructure/
├── Data/
│   ├── FacilityDbContext.cs
│   ├── Configurations/
│   │   ├── EquipmentConfiguration.cs
│   │   ├── EquipmentNoteConfiguration.cs
│   │   ├── EquipmentLoanConfiguration.cs
│   │   ├── EquipmentMaintenanceLogConfiguration.cs
│   │   └── EquipmentAttachmentConfiguration.cs
│   ├── Migrations/
│   │   └── [generated]
│   └── Repositories/
│       ├── EquipmentRepository.cs
│       ├── LoanRepository.cs
│       ├── MaintenanceLogRepository.cs
│       └── AttachmentRepository.cs
├── Services/
│   └── AssetCodeGenerator.cs
├── ExternalClients/
│   └── JobServiceClient.cs
└── DependencyInjection.cs

Maliev.FacilityService.Tests/
├── Unit/
│   ├── RegisterEquipmentCommandTests.cs
│   ├── ChangeEquipmentStatusCommandTests.cs
│   ├── DeleteEquipmentCommandTests.cs
│   ├── GetActiveEquipmentsByCategoryQueryTests.cs
│   ├── CreateLoanCommandTests.cs
│   ├── ApproveLoanCommandTests.cs
│   ├── AddMaintenanceLogCommandTests.cs
│   └── FacilityPermissionsTests.cs
└── Integration/
    ├── PostgresFixture.cs
    ├── EquipmentsControllerTests.cs
    ├── LoansControllerTests.cs
    └── MaintenanceControllerTests.cs
```

**Structure Decision**: Clean Architecture with Api / Application / Domain / Infrastructure / Tests. Five projects, each with a single responsibility. Domain has no external dependencies; Application depends only on Domain; Infrastructure implements Application interfaces; Api wires everything via DI.

---

## Key Technical Decisions

### Table-Per-Type (TPT) Schema

The database uses EF Core's TPT pattern:
- Base `equipments` table with all common fields
- Separate spec tables for manufacturing subtypes (`fdm_printer_specs`, `sla_printer_specs`, `cnc_machine_specs`, `scanner_3d_specs`, `injection_molding_specs`)
- General equipment subtypes (`OfficeEquipmentItem`, etc.) have no spec table — only the base `equipments` row

This ensures zero nullable manufacturing columns on general equipment rows.

### Asset Code Generation

Asset codes are auto-generated on registration with per-category sequences:
- Format: `MAL-{PREFIX}-{SEQ}`
- Each category has its own counter
- Immutable after creation

### Lending Workflow

- **Employee loans**: Created immediately with equipment status → `OnLoan`
- **Customer loans**: Created as `PendingApproval`, requires manager approval → equipment status → `OnLoan` + `LoanDocumentRequestedEvent` published

### Messaging Events

- `EquipmentStatusChangedEvent`: Published on every status transition
- `LoanDocumentRequestedEvent`: Published when a customer loan is approved, consumed by `Maliev.PdfService`

### Out of Scope

- Consumables (cutting tools, filament, resin, scanning spray) → `InventoryService`
- Material process parameters → `MaterialService`
- PDF generation → `Maliev.PdfService` (FacilityService publishes event only)

---

## Complexity Tracking

No constitution violations require justification. Standard 5-project Clean Architecture layout.
