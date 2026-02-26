using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Infrastructure.Data;
using Maliev.FacilityService.Infrastructure.Data.Repositories;
using Maliev.FacilityService.Infrastructure.ExternalClients;
using Maliev.FacilityService.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.FacilityService.Infrastructure;

/// <summary>
/// Extension methods for registering Infrastructure layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all infrastructure services including EF Core and repositories.
    /// MassTransit is registered separately via <c>builder.AddMassTransitWithRabbitMq()</c> in Program.cs.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FacilityDbContext")
            ?? throw new InvalidOperationException("Connection string 'FacilityDbContext' not found.");

        services.AddDbContext<FacilityDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<IEquipmentRepository, EquipmentRepository>();
        services.AddScoped<IEquipmentNoteRepository, EquipmentNoteRepository>();
        services.AddScoped<ILoanRepository, LoanRepository>();
        services.AddScoped<IMaintenanceLogRepository, MaintenanceLogRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

        services.AddScoped<IAssetCodeGenerator, AssetCodeGenerator>();

        services.AddHttpClient<IJobServiceClient, JobServiceClient>(client =>
        {
            client.BaseAddress = new Uri("http://job-service");
        });

        return services;
    }
}
