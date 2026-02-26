# Maliev Facility Service

[![Build Status](https://img.shields.io/badge/Build-Passing-success)](https://github.com/ORGANIZATION/Maliev.FacilityService)
[![.NET Version](https://img.shields.io/badge/.NET-10.0-blue)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Database](https://img.shields.io/badge/Database-PostgreSQL-blue)](https://www.postgresql.org/)

Production-ready company asset lifecycle management microservice covering all 10 equipment categories — from manufacturing machines to office IT — with lending, maintenance logs, and CNC attachment tracking.

**Role in MALIEV Architecture**: The single source of truth for all physical company assets. It tracks equipment status, manages loans to employees and customers, records maintenance history, and publishes events consumed by PricingService, JobService, and PdfService.

---

## 🏗️ Architecture & Tech Stack

- **Framework**: ASP.NET Core 10.0 (C# 13)
- **Database**: PostgreSQL with Entity Framework Core 10.x (Table-Per-Type schema)
- **Messaging**: RabbitMQ via MassTransit (transactional outbox)
- **API Documentation**: OpenAPI 3.1 + Scalar UI
- **Observability**: OpenTelemetry (Metrics, Traces, Logging)

---

## ⚖️ Constitution Rules

This service strictly adheres to the platform development mandates:

### Banned Libraries
To maintain high performance and low complexity, the following are **NOT** used:
- ❌ **AutoMapper**: Explicit manual mapping only.
- ❌ **FluentValidation**: Standard Data Annotations (`[Required]`, `[Range]`) only.
- ❌ **FluentAssertions**: Standard xUnit `Assert` methods only.
- ❌ **Swagger/Swashbuckle**: Scalar UI only.
- ❌ **In-memory Test DB**: All integration tests use **Testcontainers** with real PostgreSQL.

### Mandatory Practices
- ✅ **TreatWarningsAsErrors**: Enabled in all `.csproj` files.
- ✅ **XML Documentation**: Required on all public methods and properties.
- ✅ **No Secrets in Code**: All sensitive configuration injected via environment variables.
- ✅ **No Test Config in Program.cs**: Test configuration in test fixtures only.
- ✅ **IAM Integration**: All endpoints protected with `[RequirePermission]` using GCP-style naming: `facility.{resource}.{action}`.
- ✅ **Optimistic Concurrency**: `xmin` row-version on all write endpoints; `DbUpdateConcurrencyException` → HTTP 409.
- ✅ **Status Transitions**: Invalid transitions → HTTP 422 (not 400).
- ✅ **Fail-safe Deletes**: Hard delete blocked when JobService has history → HTTP 409; JobService unreachable → HTTP 503.

---

## ✨ Key Features

- **10 Equipment Categories**: `FdmPrinter`, `SlaPrinter`, `CncMachine`, `Scanner3D`, `InjectionMolding`, `OfficeEquipment`, `MeasuringEquipment`, `ITEquipment`, `HandTool`, `Other`.
- **Table-Per-Type (TPT) Schema**: Manufacturing machines have dedicated spec tables; general equipment rows are lean with no nullable manufacturing columns.
- **Auto-generated Asset Codes**: Format `MAL-{PREFIX}-{SEQ}` with per-category sequences on registration.
- **Equipment Lending**: Employee loans and customer loans with approval workflow. Customer loan approval publishes `LoanDocumentRequestedEvent` to trigger PDF generation.
- **Maintenance Logs**: Append-only maintenance history with optional next-service-due date updates.
- **CNC Attachments**: Track tools, fixtures, collets, and other CNC-specific accessories per machine.
- **Append-only Notes**: Timestamped equipment notes that cannot be edited or deleted.
- **Event Publishing**: `EquipmentStatusChangedEvent` on every status transition for downstream consumers.

---

## 🚀 Quick Start

### Prerequisites
- .NET 10.0 SDK
- Docker Desktop (for full local stack via Aspire)

### Local Development Setup

1. **Clone the repository**
```bash
git clone https://github.com/ORGANIZATION/Maliev.FacilityService.git
cd Maliev.FacilityService
```

2. **Configure Environment**
```powershell
# Windows PowerShell
$env:ConnectionStrings__FacilityDbContext="Host=localhost;Database=facility-app-db;Username=postgres;Password=YOUR_PASSWORD"
```

3. **Apply Migrations & Run**
```bash
dotnet ef database update --project Maliev.FacilityService.Infrastructure --startup-project Maliev.FacilityService.Api
dotnet run --project Maliev.FacilityService.Api
```

The service will be available at `http://localhost:5200/facility`. Access the interactive documentation at `http://localhost:5200/facility/scalar`.

---

## 📡 API Endpoints

All endpoints are prefixed with `/facility/v1/`.

### Equipment

| Method | Endpoint | Permission | Description |
|--------|----------|------------|-------------|
| GET | `/equipments` | `facility.equipments.read` | List all equipment with filters and pagination |
| GET | `/equipments/active` | `facility.equipments.read` | Get active equipment, optionally filtered by category |
| GET | `/equipments/{id}` | `facility.equipments.read` | Get single equipment with full spec detail |
| POST | `/equipments` | `facility.equipments.write` | Register new equipment (auto-generates asset code) |
| PUT | `/equipments/{id}` | `facility.equipments.write` | Update equipment details |
| PATCH | `/equipments/{id}/status` | `facility.equipments.write` | Change operational status |
| DELETE | `/equipments/{id}` | `facility.equipments.manage` | Hard-delete (blocked if job history exists) |
| POST | `/equipments/{id}/notes` | `facility.equipments.write` | Append a note (append-only) |
| GET | `/equipments/{id}/notes` | `facility.equipments.read` | List all notes for equipment |

### Loans

| Method | Endpoint | Permission | Description |
|--------|----------|------------|-------------|
| POST | `/loans` | `facility.loans.write` | Create loan request (customer loans enter Pending state) |
| PATCH | `/loans/{id}/approve` | `facility.loans.approve` | Approve loan (triggers PDF event for customer loans) |
| PATCH | `/loans/{id}/reject` | `facility.loans.approve` | Reject pending loan |
| PATCH | `/loans/{id}/return` | `facility.loans.write` | Record equipment return |
| GET | `/equipments/{id}/loans` | `facility.loans.read` | Get loan history for equipment |

### Maintenance

| Method | Endpoint | Permission | Description |
|--------|----------|------------|-------------|
| POST | `/equipments/{id}/maintenance` | `facility.maintenance.write` | Add maintenance log entry |
| GET | `/equipments/{id}/maintenance` | `facility.maintenance.read` | List maintenance logs (newest first) |

### Attachments (CNC)

| Method | Endpoint | Permission | Description |
|--------|----------|------------|-------------|
| GET | `/equipments/{id}/attachments` | `facility.attachments.read` | List attachments for equipment |
| POST | `/equipments/{id}/attachments` | `facility.attachments.write` | Add CNC attachment |
| PUT | `/equipments/{id}/attachments/{attachmentId}` | `facility.attachments.write` | Update attachment details |

---

## 🏥 Health & Monitoring

Standardized health probes for Kubernetes orchestration:
- **Liveness**: `GET /facility/liveness`
- **Readiness**: `GET /facility/readiness` (Checks DB connectivity)
- **Metrics**: `GET /facility/metrics` (Prometheus format)

---

## 🔐 Permissions Model

All permissions follow the GCP-style `facility.{resource}.{action}` convention:

| Permission | Description |
|------------|-------------|
| `facility.equipments.read` | Read equipment information |
| `facility.equipments.write` | Create and update equipment |
| `facility.equipments.manage` | Delete or manage equipment lifecycle |
| `facility.loans.read` | Read loan records |
| `facility.loans.write` | Create and update loans |
| `facility.loans.approve` | Approve loan requests |
| `facility.maintenance.read` | Read maintenance logs |
| `facility.maintenance.write` | Create and update maintenance logs |
| `facility.attachments.read` | Read equipment attachments |
| `facility.attachments.write` | Manage equipment attachments |

---

## 🧪 Testing

```bash
# Run all tests with coverage
dotnet test Maliev.FacilityService.Tests --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
```

- **Integration Tests**: Use real PostgreSQL containers via Testcontainers.
- **Target Coverage**: ≥ 80%.

---

## 📦 Deployment

Infrastructure management is handled via GitOps patterns.

- **Docker Image**: `REGION-docker.pkg.dev/PROJECT_ID/REPOSITORY/maliev-facility-service:{sha}`
- **Environments**: Development, Staging, Production

---

## 📄 License

Proprietary - © 2025 MALIEV Co., Ltd. All rights reserved.
