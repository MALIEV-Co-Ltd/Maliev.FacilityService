using Maliev.FacilityService.Domain.Entities;

namespace Maliev.FacilityService.Application.Interfaces;

/// <summary>
/// Repository interface for equipment maintenance log operations.
/// </summary>
public interface IMaintenanceLogRepository
{
    /// <summary>
    /// Retrieves all maintenance logs for an equipment.
    /// </summary>
    /// <param name="equipmentId">The unique identifier of the equipment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of maintenance log entities for the equipment.</returns>
    Task<IReadOnlyList<EquipmentMaintenanceLog>> GetByEquipmentIdAsync(
        Guid equipmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new maintenance log to the repository.
    /// </summary>
    /// <param name="entity">The maintenance log entity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The added maintenance log entity.</returns>
    Task<EquipmentMaintenanceLog> AddAsync(
        EquipmentMaintenanceLog entity,
        CancellationToken cancellationToken = default);
}
