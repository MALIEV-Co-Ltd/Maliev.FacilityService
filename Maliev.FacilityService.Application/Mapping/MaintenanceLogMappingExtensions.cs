using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Domain.Entities;

namespace Maliev.FacilityService.Application.Mapping;

/// <summary>
/// Manual mapping extension methods for converting maintenance log domain entities to DTOs.
/// </summary>
public static class MaintenanceLogMappingExtensions
{
    /// <summary>
    /// Maps an <see cref="EquipmentMaintenanceLog"/> entity to a <see cref="MaintenanceLogDto"/>.
    /// </summary>
    /// <param name="log">The maintenance log entity to map.</param>
    /// <returns>A <see cref="MaintenanceLogDto"/> populated from the entity.</returns>
    public static MaintenanceLogDto ToDto(this EquipmentMaintenanceLog log) =>
        new()
        {
            Id = log.Id,
            EquipmentId = log.EquipmentId,
            Type = log.Type,
            Description = log.Description,
            OccurredAt = log.OccurredAt,
            LoggedByEmployeeId = log.LoggedByEmployeeId,
            VendorName = log.VendorName,
            CostTHB = log.CostTHB,
            NextServiceDueDate = log.NextServiceDueDate,
            CreatedAt = log.CreatedAt
        };
}
