using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.Mapping;

namespace Maliev.FacilityService.Application.UseCases.Queries.GetMaintenanceLogs;

/// <summary>
/// Handler for the <see cref="GetMaintenanceLogsQuery"/>.
/// Returns all maintenance log entries for the specified equipment, ordered by date descending.
/// </summary>
public class GetMaintenanceLogsQueryHandler
{
    private readonly IMaintenanceLogRepository _maintenanceLogRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="GetMaintenanceLogsQueryHandler"/>.
    /// </summary>
    /// <param name="maintenanceLogRepository">The maintenance log repository.</param>
    public GetMaintenanceLogsQueryHandler(IMaintenanceLogRepository maintenanceLogRepository)
    {
        _maintenanceLogRepository = maintenanceLogRepository;
    }

    /// <summary>
    /// Handles retrieval of maintenance logs for equipment.
    /// </summary>
    /// <param name="query">The get maintenance logs query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of maintenance log DTOs for the equipment.</returns>
    public async Task<IReadOnlyList<MaintenanceLogDto>> HandleAsync(
        GetMaintenanceLogsQuery query,
        CancellationToken cancellationToken = default)
    {
        var logs = await _maintenanceLogRepository.GetByEquipmentIdAsync(query.EquipmentId, cancellationToken);
        return logs.Select(l => l.ToDto()).ToList();
    }
}
