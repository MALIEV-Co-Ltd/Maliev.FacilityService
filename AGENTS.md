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

## Build, Test & Lint Commands

All commands run from `B:\maliev\Maliev.FacilityService`.

```powershell
# Build (treats warnings as errors — all must be fixed)
dotnet build Maliev.FacilityService.slnx

# Run all tests
dotnet test Maliev.FacilityService.slnx --verbosity normal

# Run a single test method
dotnet test --filter "FullyQualifiedName~EquipmentServiceTests.CreateAsync_ValidEquipment_ReturnsCreated"

# Run all tests in a class
dotnet test --filter "FullyQualifiedName~EquipmentServiceTests"

# Run with code coverage
dotnet test Maliev.FacilityService.slnx --collect:"XPlat Code Coverage"

# Format check
dotnet format Maliev.FacilityService.slnx

# EF Core migrations (Infrastructure project only)
dotnet ef migrations add <Name> --project Maliev.FacilityService.Infrastructure --startup-project Maliev.FacilityService.Infrastructure
```

## Package Management Rules

- **EF Core Design**: `Microsoft.EntityFrameworkCore.Design` must ONLY be in the Infrastructure project. Do NOT add it to the Api project. Use `--startup-project Maliev.FacilityService.Infrastructure` when running migrations - the package will be resolved transitively from Infrastructure. The Infrastructure project must NOT have `<PrivateAssets>all</PrivateAssets>` on this package.

---

## Code Style & Conventions

### C# Naming & Formatting

- **Namespaces**: File-scoped (`namespace Maliev.FacilityService.Domain.Entities;`)
- **Classes/Methods/Properties**: `PascalCase`
- **Private fields**: `_camelCase` (underscore prefix)
- **Parameters/locals**: `camelCase`
- **Async methods**: Suffix with `Async` (e.g., `RegisterEquipmentAsync`)
- **Interfaces**: Prefix with `I` (e.g., `IEquipmentRepository`)
- **Permissions**: GCP-style `facility.{plural-resource}.{action}` as `public const string` in `FacilityPermissions` static class
  - Valid: `facility.equipment.create`, `facility.maintenance-logs.delete`
  - Invalid: `facility.equipment.create` if resource is singular (should be plural), `facility.create` (missing resource)
- **XML docs**: Required on ALL public methods and properties
- **Nullable**: Enabled (`<Nullable>enable</Nullable>`). Use `?` explicitly
- **Imports**: System first, then third-party, then local. Alphabetize within groups. Remove unused `using`
- **Braces**: Allman style (new line) for methods and control structures. Expression-bodied for properties/accessors
- **Indentation**: 4 spaces, LF line endings, UTF-8, trim trailing whitespace
- **Enum values**: Stored as strings in DB via `.HasConversion<string>()`

### C# Patterns

- **DI**: Constructor injection with `private readonly` fields
- **Controllers**: `[ApiController]`, `[ApiVersion("1")]`, `[Route("facility/v{version:apiVersion}")]`
- **Logging**: `ILogger<T>` with structured placeholders (never interpolate): `_logger.LogInformation("Processing {EquipmentId}", equipmentId)`
- **Error handling**: Global exception middleware. Return `ProblemDetails` / `ErrorResponse` DTOs. Never expose stack traces
- **JSON**: Check existing conventions in this service for naming policy
- **Manual mapping**: Static extension methods (`ToDto()`, `ToEntity()`). AutoMapper is banned
- **Validation**: `System.ComponentModel.DataAnnotations` on DTOs. FluentValidation is banned
- **Concurrency**: `uint RowVersion` in all update/patch request DTOs (xmin concurrency token). Catch `DbUpdateConcurrencyException` in global middleware → HTTP 409
- **Status transitions**: Invalid status transitions → HTTP 422 (not 400)
- **Hard delete**: Blocked when JobService has history → HTTP 409; JobService unreachable → HTTP 503 (fail-safe)

---

## Banned Libraries (Build Will Fail)

| Banned | Use Instead |
|--------|-------------|
| AutoMapper | Manual mapping extensions |
| FluentValidation | DataAnnotations or manual validation |
| FluentAssertions | Standard xUnit `Assert.*` |
| Swashbuckle/Swagger | Scalar (at `/facility/scalar`) |
| InMemoryDatabase (EF Core) | Testcontainers with real PostgreSQL |

---

## Testing Rules

- **Framework**: xUnit with standard `Assert` (`Assert.Equal`, `Assert.NotNull`, etc.)
- **Naming**: `MethodName_StateUnderTest_ExpectedBehavior` or `HTTP_METHOD_Path_Scenario_ExpectedStatus`
- **Coverage**: Minimum 80% per service
- **Integration tests**: `BaseIntegrationTestFactory<TProgram, TDbContext>` with Testcontainers (PostgreSQL, Redis, RabbitMQ). Never InMemoryDatabase
- **System tests** (Tier 3): `AspireTestFixture` with `[Collection("AspireDomainTests")]` — shared AppHost, never one per class
- **Eventual consistency**: Use `TestHelpers.WaitForAsync`. Never `Task.Delay`
- **MassTransit consumers**: Must have consumer tests using `AddMassTransitTestHarness()`
- Use `[Fact]` for single cases, `[Theory]` for parameterized tests

> Full ecosystem test strategy: `Maliev.Aspire.Tests/TEST_PLAN.md`

### Testing Strategy (4-Tier Pyramid Context)

This service's tests cover **Tier 1 (Unit)** and **Tier 2 (Service Integration)** of the Maliev testing pyramid:

| Tier | What to Test | Infrastructure |
|------|-------------|---------------|
| **Unit** | Business logic, domain models, service methods with mocked dependencies | None (mocks only) |
| **Service Integration** | API endpoints, database persistence, permission enforcement, input validation | `BaseIntegrationTestFactory` + Testcontainers (Postgres/Redis/RabbitMQ) |

**Tier 3 (System Integration)** — cross-service workflows and event chains — is tested in `Maliev.Aspire.Tests/`.

---

## Mandatory Rules

- **`TreatWarningsAsErrors = true`**: Zero warnings allowed. No suppression
- **`[RequirePermission("facility.resources.action")]`**: On all endpoints, not plain `[Authorize]`
- **API versioning**: All routes versioned (`v1/`)
- **Service prefix**: Routes prefixed with `/facility`
- **Scalar docs**: Configured at `/facility/scalar`
- **Secrets**: Never hardcoded. Use GCP Secret Manager or environment variables
- **Async/await**: All the way down. Pass `CancellationToken`
- **EF Core Design package**: Only in Infrastructure project, never in Api
- **PostgreSQL xmin**: Shadow property only — `entity.Property<uint>("xmin").HasColumnType("xid").IsRowVersion()`. Never add entity property
- **Temporary files**: Generate in `/temp` folder, clean up afterwards

---

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

<!-- MANUAL ADDITIONS START -->
<!-- MANUAL ADDITIONS END -->

---

## Git Rules

- Each `Maliev.*` folder is an independent git repo. `cd` into it before git commands
- **Commit early and often** after every meaningful unit of work. Do not accumulate changes
- **Never use `git checkout` to restore files** — commit first, then `git revert` or `git reset --soft`
- Feature branches merged to `develop` via PR. Do not push without being asked
