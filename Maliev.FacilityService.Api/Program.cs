using Maliev.FacilityService.Api.Middleware;
using Maliev.FacilityService.Api.Services;
using Maliev.FacilityService.Application;
using Maliev.FacilityService.Infrastructure;
using Maliev.FacilityService.Infrastructure.Data;
using Maliev.FacilityService.Infrastructure.Data.SeedData;
using Maliev.Aspire.ServiceDefaults.IAM;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// --- Infrastructure & Observability ---
builder.AddServiceDefaults(); // OTel, health checks, resilience — DO NOT configure manually
builder.AddDefaultApiVersioning(); // URL-segment versioning (e.g. /facility/v1/...)

// --- Application Services ---
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// --- Messaging (MassTransit + RabbitMQ + EF Core transactional outbox) ---
builder.AddMassTransitWithRabbitMq(configure: x =>
{
    x.AddEntityFrameworkOutbox<FacilityDbContext>(o =>
    {
        o.UsePostgres();
        o.UseBusOutbox();
    });
});

// --- IAM Registration ---
builder.Services.AddIAMRegistration<FacilityIAMRegistrationService>("facility");

// --- API Configuration ---
builder.Services.AddControllers();

// --- Authentication & Authorization ---
builder.AddJwtAuthentication();

// --- API Documentation (Scalar, not Swagger) ---
builder.AddStandardOpenApi(
    title: "MALIEV Facility Service API",
    description: "Company-wide asset lifecycle management service. Handles equipment registration, status tracking, lending, maintenance logs, and CNC attachments across all 10 equipment categories.");

var app = builder.Build();

// --- Database Migration ---
await app.MigrateDatabaseAsync<FacilityDbContext>();

// --- Seed Equipment Data ---
await app.SeedEquipmentsAsync();

// --- Middleware Pipeline ---
app.UseMiddleware<DomainExceptionMiddleware>();
app.UseMiddleware<ConcurrencyExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// --- Endpoints ---
app.MapControllers();

// Health check, liveness, readiness, and Prometheus metrics endpoints
app.MapDefaultEndpoints(servicePrefix: "facility");

// OpenAPI + Scalar documentation (dev/staging only)
app.MapApiDocumentation(servicePrefix: "facility");

app.Run();

/// <summary>Exposes Program for WebApplicationFactory in integration tests.</summary>
public partial class Program { }
