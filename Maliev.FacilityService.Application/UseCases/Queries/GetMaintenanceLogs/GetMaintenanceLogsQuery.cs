namespace Maliev.FacilityService.Application.UseCases.Queries.GetMaintenanceLogs;

/// <summary>
/// Query to retrieve all maintenance log entries for a specific equipment.
/// </summary>
/// <param name="EquipmentId">The unique identifier of the equipment.</param>
public record GetMaintenanceLogsQuery(Guid EquipmentId);
