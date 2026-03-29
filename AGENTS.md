# Maliev.FacilityService Development Guidelines

Auto-generated from all feature plans. Last updated: 2026-02-25

## Active Technologies

- **Language**: C# / .NET 10
- **Web framework**: ASP.NET Core 10 (minimal API + controllers)
- **ORM**: EF Core 10 + Npgsql.EntityFrameworkCore.PostgreSQL
- **Messaging**: MassTransit 8.x + RabbitMQ + MassTransit.EntityFrameworkCore (transactional outbox)
- **Service defaults**: Maliev.Aspire.ServiceDefaults (OTel, health checks, Polly — do NOT configure manually)
- **Database**: PostgreSQL (dedicated `facility-app-db`); Table-Per-Type (TPT) schema with typed spec tables per manufacturing subtype; `xmin` row-version for optimistic concurrency; MassTransit outbox tables
- **API docs**: Scalar (NOT Swagger/Swashbuckle — banned)
- **Testing**: xUnit, Moq, Testcontainers.PostgreSql, MassTransit.TestFramework; coverlet; target ≥ 80%

## Project Structure

```text
Maliev.FacilityService.slnx

Maliev.FacilityService.Api/            # Controllers, Program.cs, DI wiring, Dockerfile
Maliev.FacilityService.Application/   # Use cases, interfaces, DTOs, validators
Maliev.FacilityService.Domain/       # Equipment entities (TPT), enums, permissions, exceptions
Maliev.FacilityService.Infrastructure/ # FacilityDbContext, migrations, repositories, external clients
Maliev.FacilityService.Tests/        # Unit/ + Integration/ (Testcontainers)
```

Full directory structure: see `specs/001-equipment-management/plan.md`

## Commands

```bash
# Build
dotnet build Maliev.FacilityService.slnx

# Run (via Aspire AppHost for full local stack)
dotnet run --project Maliev.Aspire

# Run tests with coverage
dotnet test Maliev.FacilityService.Tests --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover

# Add EF Core migration
dotnet ef migrations add <MigrationName> --project Maliev.FacilityService.Infrastructure --startup-project Maliev.FacilityService.Infrastructure --output-dir Data/Migrations

# Update database (local dev)
dotnet ef database update --project Maliev.FacilityService.Infrastructure --startup-project Maliev.FacilityService.Infrastructure
```

## Package Management Rules

- **EF Core Design**: `Microsoft.EntityFrameworkCore.Design` must ONLY be in the Infrastructure project. Do NOT add it to the Api project. Use `--startup-project Maliev.FacilityService.Infrastructure` when running migrations - the package will be resolved transitively from Infrastructure. The Infrastructure project must NOT have `<PrivateAssets>all</PrivateAssets>` on this package.

## Code Style

- All public methods, properties, and classes require XML documentation comments (`///`)
- `TreatWarningsAsErrors = true` — never suppress warnings
- Use `[RequirePermission("facility.{resource}.{action}")]` on every endpoint — never plain `[Authorize]`
- Permission strings defined as `public const string` in `FacilityPermissions` static class
- Enum values stored as strings in DB via `.HasConversion<string>()`
- `uint RowVersion` in all update/patch request DTOs (xmin concurrency token)
- Catch `DbUpdateConcurrencyException` in global middleware → HTTP 409
- Invalid status transitions → HTTP 422 (not 400)
- Hard delete blocked when JobService has history → HTTP 409; JobService unreachable → HTTP 503 (fail-safe)
- All async/await throughout; no blocking calls
- No `AutoMapper`, `FluentValidation`, `FluentAssertions`, `Serilog`

## Data Model: Table-Per-Type (TPT)

The database uses EF Core's TPT pattern:
- Base `equipments` table with all common fields
- Separate spec tables for manufacturing subtypes: `fdm_printer_specs`, `sla_printer_specs`, `cnc_machine_specs`, `scanner_3d_specs`, `injection_molding_specs`
- General equipment (`OfficeEquipment`, `ITEquipment`, `MeasuringEquipment`, etc.) has no spec table — only the base row
- This ensures zero nullable manufacturing columns on general equipment rows

## Equipment Categories

10 categories supported: `FdmPrinter`, `SlaPrinter`, `CncMachine`, `Scanner3D`, `InjectionMolding`, `OfficeEquipment`, `MeasuringEquipment`, `ITEquipment`, `HandTool`, `Other`

## Asset Code Generation

Asset codes are auto-generated on registration: `MAL-{PREFIX}-{SEQ}` with per-category sequences.

## Capacity Planning

FacilityService maintains full capacity planning data for manufacturing operations:
- **Machine availability** — real-time status of all equipment (Active, UnderMaintenance, etc.)
- **Scheduling** — time slots, availability windows per machine
- **Maintenance tracking** — scheduled maintenance, downtime history
- **Queue depth** — current and projected queue per technology (FDM, SLA, CNC)
- **Lead time estimates** — based on current load and historical data
- **JobService integration** — provides real-time capacity visibility to job scheduling

## Status Values

`Active`, `UnderMaintenance`, `OnLoan`, `Lost`, `Decommissioned` (terminal)

## Events Published

- `EquipmentStatusChangedEvent` — on every status transition
- `LoanDocumentRequestedEvent` — when a customer loan is approved (consumed by `Maliev.PdfService`)

## Recent Changes

- `001-equipment-management` (2026-02-25): **Major scope revision** — Full company asset lifecycle management (not just manufacturing machines). Added: 10 equipment categories, TPT database schema, asset code auto-generation, equipment notes (append-only), equipment lending (employee + customer with approval + PDF event), maintenance logs, CNC attachments, new permissions (loans, maintenance, attachments), new Technician role.

### Testing Strategy (4-Tier Pyramid Context)

This service's tests cover **Tier 1 (Unit)** and **Tier 2 (Service Integration)** of the Maliev testing pyramid:

| Tier | What to Test | Infrastructure |
|------|-------------|---------------|
| **Unit** | Business logic, domain models, service methods with mocked dependencies | None (mocks only) |
| **Service Integration** | API endpoints, database persistence, permission enforcement, input validation | `BaseIntegrationTestFactory` + Testcontainers (Postgres/Redis/RabbitMQ) |

**Tier 3 (System Integration)** — cross-service workflows and event chains — is tested in `Maliev.Aspire.Tests/`.

#### Key Rules
- Use `BaseIntegrationTestFactory<TProgram, TDbContext>` for integration tests (real Testcontainers, never InMemoryDatabase)
- Test naming: `MethodName_StateUnderTest_ExpectedBehavior`
- Minimum 80% code coverage
- Use `[Fact]` for single cases, `[Theory]` for parameterized tests

> Full ecosystem test strategy: `Maliev.Aspire.Tests/TEST_PLAN.md`

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->

---

## Git & Version Control — Mandatory Rules

### 🚨 CRITICAL: Always Commit Code Changes (Non-Negotiable)
- **You MUST commit your changes to the local repository after completing any meaningful unit of work.**
- **Never accumulate uncommitted changes.** Do not wait until end of session or until something breaks.
- **Commit early and often** — if a change is meaningful (even a small fix or refactor), commit it.
- **You do NOT need to push to remote** — local commits are sufficient to protect against accidental loss.
- **If you are unsure whether to commit, commit anyway.** Extra commits are harmless; lost work is irreversible.
- This rule applies even if you are just "testing" or "exploring" — use git branches to isolate experimental work and commit those changes too.

### 🚨 CRITICAL: Never Use `git checkout` to Restore Broken Files
- **NEVER use `git checkout` to restore or recover files.** This operation discards uncommitted changes permanently and will result in data loss.
- **To undo/recover from broken files: first commit your current changes, then use `git revert` or `git reset --soft` to safely undo.
