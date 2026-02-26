using Maliev.FacilityService.Application.UseCases.Commands.AddAttachment;
using Maliev.FacilityService.Application.UseCases.Commands.AddEquipmentNote;
using Maliev.FacilityService.Application.UseCases.Commands.AddMaintenanceLog;
using Maliev.FacilityService.Application.UseCases.Commands.ApproveLoan;
using Maliev.FacilityService.Application.UseCases.Commands.ChangeEquipmentStatus;
using Maliev.FacilityService.Application.UseCases.Commands.CreateLoan;
using Maliev.FacilityService.Application.UseCases.Commands.DeleteEquipment;
using Maliev.FacilityService.Application.UseCases.Commands.RegisterEquipment;
using Maliev.FacilityService.Application.UseCases.Commands.RejectLoan;
using Maliev.FacilityService.Application.UseCases.Commands.ReturnLoan;
using Maliev.FacilityService.Application.UseCases.Commands.UpdateAttachment;
using Maliev.FacilityService.Application.UseCases.Commands.UpdateEquipment;
using Maliev.FacilityService.Application.UseCases.Queries.GetActiveEquipmentsByCategory;
using Maliev.FacilityService.Application.UseCases.Queries.GetAttachments;
using Maliev.FacilityService.Application.UseCases.Queries.GetEquipmentById;
using Maliev.FacilityService.Application.UseCases.Queries.GetEquipmentLoans;
using Maliev.FacilityService.Application.UseCases.Queries.GetEquipmentNotes;
using Maliev.FacilityService.Application.UseCases.Queries.GetMaintenanceLogs;
using Maliev.FacilityService.Application.UseCases.Queries.ListEquipments;
using Microsoft.Extensions.DependencyInjection;

namespace Maliev.FacilityService.Application;

/// <summary>
/// Extension methods for registering Application layer services in the DI container.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers all Application layer command handlers and query handlers as scoped services.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Command handlers
        services.AddScoped<RegisterEquipmentCommandHandler>();
        services.AddScoped<UpdateEquipmentCommandHandler>();
        services.AddScoped<ChangeEquipmentStatusCommandHandler>();
        services.AddScoped<DeleteEquipmentCommandHandler>();
        services.AddScoped<AddEquipmentNoteCommandHandler>();
        services.AddScoped<CreateLoanCommandHandler>();
        services.AddScoped<ApproveLoanCommandHandler>();
        services.AddScoped<RejectLoanCommandHandler>();
        services.AddScoped<ReturnLoanCommandHandler>();
        services.AddScoped<AddMaintenanceLogCommandHandler>();
        services.AddScoped<AddAttachmentCommandHandler>();
        services.AddScoped<UpdateAttachmentCommandHandler>();

        // Query handlers
        services.AddScoped<GetEquipmentByIdQueryHandler>();
        services.AddScoped<ListEquipmentsQueryHandler>();
        services.AddScoped<GetActiveEquipmentsByCategoryQueryHandler>();
        services.AddScoped<GetEquipmentNotesQueryHandler>();
        services.AddScoped<GetEquipmentLoansQueryHandler>();
        services.AddScoped<GetMaintenanceLogsQueryHandler>();
        services.AddScoped<GetAttachmentsQueryHandler>();

        return services;
    }
}
