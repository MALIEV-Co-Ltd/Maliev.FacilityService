using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.UseCases.Commands.AddMaintenanceLog;
using Maliev.FacilityService.Application.UseCases.Queries.GetMaintenanceLogs;
using Maliev.FacilityService.Domain.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.FacilityService.Api.Controllers;

/// <summary>
/// Manages maintenance log entries for equipment.
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("facility/v{version:apiVersion}/equipments/{id:guid}/maintenance")]
public class MaintenanceController : FacilityControllerBase
{
    private readonly AddMaintenanceLogCommandHandler _addHandler;
    private readonly GetMaintenanceLogsQueryHandler _getHandler;

    /// <summary>
    /// Initializes a new instance of <see cref="MaintenanceController"/>.
    /// </summary>
    /// <param name="addHandler">Handler for adding maintenance log entries.</param>
    /// <param name="getHandler">Handler for retrieving maintenance logs.</param>
    public MaintenanceController(
        AddMaintenanceLogCommandHandler addHandler,
        GetMaintenanceLogsQueryHandler getHandler)
    {
        _addHandler = addHandler;
        _getHandler = getHandler;
    }

    /// <summary>
    /// Adds a maintenance log entry for equipment.
    /// Optionally updates the next service due date on the equipment record.
    /// </summary>
    /// <param name="id">The unique identifier of the equipment.</param>
    /// <param name="command">The maintenance log details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created maintenance log DTO.</returns>
    [HttpPost]
    [RequirePermission(FacilityPermissions.MaintenanceWrite)]
    public async Task<ActionResult<MaintenanceLogDto>> AddMaintenanceLog(
        [FromRoute] Guid id,
        [FromBody] AddMaintenanceLogCommand command,
        CancellationToken cancellationToken = default)
    {
        var cmdWithId = command with { EquipmentId = id };
        var result = await _addHandler.HandleAsync(cmdWithId, cancellationToken);
        return Created($"facility/v{ApiVersion}/equipments/{id}/maintenance", result);
    }

    /// <summary>
    /// Retrieves all maintenance log entries for equipment, ordered by occurrence date descending.
    /// </summary>
    /// <param name="id">The unique identifier of the equipment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of maintenance log DTOs.</returns>
    [HttpGet]
    [RequirePermission(FacilityPermissions.MaintenanceRead)]
    public async Task<ActionResult<IReadOnlyList<MaintenanceLogDto>>> GetMaintenanceLogs(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _getHandler.HandleAsync(
            new GetMaintenanceLogsQuery(id),
            cancellationToken);
        return Ok(result);
    }
}
