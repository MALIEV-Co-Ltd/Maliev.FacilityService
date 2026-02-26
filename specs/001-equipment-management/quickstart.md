# Quickstart: Facility & Equipment Management

**Branch**: `001-equipment-management` | **Date**: 2026-02-25

---

## Prerequisites

- .NET 10 SDK
- Docker Desktop (for Testcontainers + local Aspire)
- Access to the MALIEV private NuGet feed (credentials via environment variables — never hardcoded)
- `Maliev.MessagingContracts` built locally (for `EquipmentStatusChangedEvent` C# record)
- `Maliev.Aspire.ServiceDefaults` available via private NuGet feed

---

## Step 1: Create the Solution

```bash
# From the repo root
dotnet new slnx -n Maliev.FacilityService
```

---

## Step 2: Create Projects

```bash
dotnet new webapi -n Maliev.FacilityService.Api --no-openapi
dotnet new classlib -n Maliev.FacilityService.Application
dotnet new classlib -n Maliev.FacilityService.Domain
dotnet new classlib -n Maliev.FacilityService.Infrastructure
dotnet new xunit -n Maliev.FacilityService.Tests

# Add all projects to the solution
dotnet slnx add Maliev.FacilityService.Api
dotnet slnx add Maliev.FacilityService.Application
dotnet slnx add Maliev.FacilityService.Domain
dotnet slnx add Maliev.FacilityService.Infrastructure
dotnet slnx add Maliev.FacilityService.Tests

# Delete default boilerplate immediately
# Remove: Class1.cs, WeatherForecast.cs, UnitTest1.cs, any generated example controllers
```

---

## Step 3: Add Project References

```bash
# Application depends on Domain
dotnet add Maliev.FacilityService.Application reference Maliev.FacilityService.Domain

# Infrastructure depends on Application + Domain
dotnet add Maliev.FacilityService.Infrastructure reference Maliev.FacilityService.Application
dotnet add Maliev.FacilityService.Infrastructure reference Maliev.FacilityService.Domain

# Api depends on Application + Infrastructure
dotnet add Maliev.FacilityService.Api reference Maliev.FacilityService.Application
dotnet add Maliev.FacilityService.Api reference Maliev.FacilityService.Infrastructure

# Tests depends on all layers
dotnet add Maliev.FacilityService.Tests reference Maliev.FacilityService.Api
dotnet add Maliev.FacilityService.Tests reference Maliev.FacilityService.Application
dotnet add Maliev.FacilityService.Tests reference Maliev.FacilityService.Domain
dotnet add Maliev.FacilityService.Tests reference Maliev.FacilityService.Infrastructure
```

---

## Step 4: Add NuGet Packages

**Api project:**
```bash
dotnet add Maliev.FacilityService.Api package Maliev.Aspire.ServiceDefaults
dotnet add Maliev.FacilityService.Api package Scalar.AspNetCore
```

**Infrastructure project:**
```bash
dotnet add Maliev.FacilityService.Infrastructure package Microsoft.EntityFrameworkCore
dotnet add Maliev.FacilityService.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add Maliev.FacilityService.Infrastructure package MassTransit.RabbitMQ
dotnet add Maliev.FacilityService.Infrastructure package MassTransit.EntityFrameworkCore
```

**Tests project:**
```bash
dotnet add Maliev.FacilityService.Tests package xunit
dotnet add Maliev.FacilityService.Tests package xunit.runner.visualstudio
dotnet add Maliev.FacilityService.Tests package Microsoft.NET.Test.Sdk
dotnet add Maliev.FacilityService.Tests package Moq
dotnet add Maliev.FacilityService.Tests package coverlet.collector
dotnet add Maliev.FacilityService.Tests package Testcontainers.PostgreSql
dotnet add Maliev.FacilityService.Tests package MassTransit.TestFramework
```

---

## Step 5: Add Messaging Contracts

Before writing any event publisher code:

1. In `Maliev.MessagingContracts`, add `contracts/schemas/facility/equipment-status-changed-event.json` (schema in `contracts/messaging-contracts.md`).
2. Run `./scripts/build.ps1` in `Maliev.MessagingContracts` to generate the C# `EquipmentStatusChangedEvent` record.
3. Add the `Maliev.MessagingContracts` package reference to `Maliev.FacilityService.Infrastructure`.

---

## Step 6: First Migration

After implementing `FacilityDbContext` and `EquipmentConfiguration`:

```bash
dotnet ef migrations add InitialCreate \
  --project Maliev.FacilityService.Infrastructure \
  --startup-project Maliev.FacilityService.Api \
  --output-dir Data/Migrations
```

---

## Step 7: Register in Aspire AppHost

In `Maliev.Aspire` project `Program.cs`:

```csharp
var facilityDb = postgres.AddDatabase("facility-app-db");

var facilityService = builder.AddProject<Projects.Maliev_FacilityService_Api>("facility-service")
    .WithReference(facilityDb)
    .WithReference(rabbitmq)
    .WithReference(redis)
    .WithEnvironment("JobService__BaseUrl", jobService.GetEndpoint("http"));
```

Connection string name in `appsettings.json`: `"FacilityDbContext"`

---

## Step 8: Run Tests

```bash
dotnet test Maliev.FacilityService.Tests \
  --collect:"XPlat Code Coverage" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
```

Coverage target: **≥ 80%**

---

## Key Patterns to Follow

| Concern | Pattern |
|---|---|
| All endpoints | `[RequirePermission("facility.equipments.{action}")]` — never plain `[Authorize]` |
| Optimistic concurrency | `uint rowVersion` in request body; catch `DbUpdateConcurrencyException` → 409 in middleware |
| Status changes | `PATCH /status` only — never allow status update via `PUT` |
| Event publishing | Via `IPublishEndpoint` (MassTransit outbox — never publish directly to RabbitMQ) |
| Process parameters | Validate required keys per technology in `ProcessParameterValidator` before persisting |
| Hard delete guard | Call `IJobServiceClient.HasHistoricalJobsAsync` first; fail-safe (block) on 5xx or timeout |
| Logging | Via `ILogger<T>` from `Microsoft.Extensions.Logging` — never Serilog |
| API docs | XML comments on all public members; Scalar at `/facility/scalar` |
