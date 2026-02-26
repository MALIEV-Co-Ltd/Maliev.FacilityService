using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Application.UseCases.Commands.AddMaintenanceLog;

/// <summary>
/// Command to add a maintenance log entry for equipment.
/// </summary>
/// <param name="EquipmentId">ID of the equipment. Set by the controller from the route.</param>
/// <param name="Type">Type of maintenance performed.</param>
/// <param name="Description">Description of the maintenance work.</param>
/// <param name="OccurredAt">Date and time when the maintenance occurred (UTC).</param>
/// <param name="LoggedByEmployeeId">ID of the employee logging the maintenance.</param>
/// <param name="VendorName">Name of the vendor who performed the maintenance.</param>
/// <param name="CostTHB">Cost of maintenance in Thai Baht.</param>
/// <param name="NextServiceDueDate">Next scheduled service date.</param>
public record AddMaintenanceLogCommand(
    Guid EquipmentId,
    MaintenanceType Type,
    string Description,
    DateTime OccurredAt,
    Guid LoggedByEmployeeId,
    string? VendorName,
    decimal? CostTHB,
    DateOnly? NextServiceDueDate);
