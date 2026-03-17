using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.UseCases.Commands.AddEquipmentNote;
using Maliev.FacilityService.Application.UseCases.Commands.ChangeEquipmentStatus;
using Maliev.FacilityService.Application.UseCases.Commands.DeleteEquipment;
using Maliev.FacilityService.Application.UseCases.Commands.RegisterEquipment;
using Maliev.FacilityService.Application.UseCases.Commands.UpdateEquipment;
using Maliev.FacilityService.Application.UseCases.Queries.GetActiveEquipmentsByCategory;
using Maliev.FacilityService.Application.UseCases.Queries.GetEquipmentById;
using Maliev.FacilityService.Application.UseCases.Queries.GetEquipmentNotes;
using Maliev.FacilityService.Application.UseCases.Queries.ListEquipments;
using Maliev.FacilityService.Domain.Authorization;
using Maliev.FacilityService.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.FacilityService.Api.Controllers;

/// <summary>
/// Manages equipment registration, lifecycle, notes, and queries across all 10 equipment categories.
/// </summary>
[ApiController]
[ApiVersion("1")]
[Route("facility/v{version:apiVersion}/equipments")]
public class EquipmentsController : ControllerBase
{
    private readonly RegisterEquipmentCommandHandler _registerHandler;
    private readonly UpdateEquipmentCommandHandler _updateHandler;
    private readonly ChangeEquipmentStatusCommandHandler _changeStatusHandler;
    private readonly DeleteEquipmentCommandHandler _deleteHandler;
    private readonly AddEquipmentNoteCommandHandler _addNoteHandler;
    private readonly GetEquipmentByIdQueryHandler _getByIdHandler;
    private readonly ListEquipmentsQueryHandler _listHandler;
    private readonly GetActiveEquipmentsByCategoryQueryHandler _getActiveHandler;
    private readonly GetEquipmentNotesQueryHandler _getNotesHandler;

    /// <summary>
    /// Initializes a new instance of <see cref="EquipmentsController"/>.
    /// </summary>
    /// <param name="registerHandler">Handler for registering equipment.</param>
    /// <param name="updateHandler">Handler for updating equipment.</param>
    /// <param name="changeStatusHandler">Handler for changing equipment status.</param>
    /// <param name="deleteHandler">Handler for deleting equipment.</param>
    /// <param name="addNoteHandler">Handler for adding equipment notes.</param>
    /// <param name="getByIdHandler">Handler for retrieving equipment by ID.</param>
    /// <param name="listHandler">Handler for listing equipment.</param>
    /// <param name="getActiveHandler">Handler for retrieving active equipment by category.</param>
    /// <param name="getNotesHandler">Handler for retrieving equipment notes.</param>
    public EquipmentsController(
        RegisterEquipmentCommandHandler registerHandler,
        UpdateEquipmentCommandHandler updateHandler,
        ChangeEquipmentStatusCommandHandler changeStatusHandler,
        DeleteEquipmentCommandHandler deleteHandler,
        AddEquipmentNoteCommandHandler addNoteHandler,
        GetEquipmentByIdQueryHandler getByIdHandler,
        ListEquipmentsQueryHandler listHandler,
        GetActiveEquipmentsByCategoryQueryHandler getActiveHandler,
        GetEquipmentNotesQueryHandler getNotesHandler)
    {
        _registerHandler = registerHandler;
        _updateHandler = updateHandler;
        _changeStatusHandler = changeStatusHandler;
        _deleteHandler = deleteHandler;
        _addNoteHandler = addNoteHandler;
        _getByIdHandler = getByIdHandler;
        _listHandler = listHandler;
        _getActiveHandler = getActiveHandler;
        _getNotesHandler = getNotesHandler;
    }

    /// <summary>
    /// Lists all equipment with optional filters and pagination.
    /// </summary>
    /// <param name="category">Optional category filter.</param>
    /// <param name="status">Optional status filter.</param>
    /// <param name="search">Optional search term (name, brand, asset code).</param>
    /// <param name="page">Page number (1-based, default 1).</param>
    /// <param name="pageSize">Items per page (default 20).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged list of equipment summaries.</returns>
    [HttpGet]
    [RequirePermission(FacilityPermissions.EquipmentsRead)]
    public async Task<ActionResult<PagedResult<EquipmentSummaryDto>>> List(
        [FromQuery] EquipmentCategory? category,
        [FromQuery] EquipmentStatus? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _listHandler.HandleAsync(
            new ListEquipmentsQuery(category, status, search, page, pageSize),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets active manufacturing equipment, optionally filtered by category.
    /// Used by PricingService and JobService for machine availability checks.
    /// </summary>
    /// <param name="category">Optional category filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of active equipment DTOs with pricing data.</returns>
    [HttpGet("active")]
    [RequirePermission(FacilityPermissions.EquipmentsRead)]
    public async Task<ActionResult<IReadOnlyList<ActiveEquipmentDto>>> GetActive(
        [FromQuery] EquipmentCategory? category,
        CancellationToken cancellationToken = default)
    {
        var result = await _getActiveHandler.HandleAsync(
            new GetActiveEquipmentsByCategoryQuery(category),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets a single equipment record by ID with full detail including spec data.
    /// </summary>
    /// <param name="id">The unique identifier of the equipment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The full equipment DTO.</returns>
    [HttpGet("{id:guid}")]
    [RequirePermission(FacilityPermissions.EquipmentsRead)]
    public async Task<ActionResult<EquipmentDto>> GetById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _getByIdHandler.HandleAsync(
            new GetEquipmentByIdQuery(id),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Registers a new piece of equipment and auto-generates an asset code.
    /// </summary>
    /// <param name="command">The registration details including category and optional spec data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created equipment DTO with generated asset code.</returns>
    [HttpPost]
    [RequirePermission(FacilityPermissions.EquipmentsWrite)]
    public async Task<ActionResult<EquipmentDto>> Register(
        [FromBody] RegisterEquipmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _registerHandler.HandleAsync(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates equipment details. Requires the current xmin row version for concurrency control.
    /// </summary>
    /// <param name="id">The unique identifier of the equipment to update.</param>
    /// <param name="command">The update details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated equipment DTO.</returns>
    [HttpPut("{id:guid}")]
    [RequirePermission(FacilityPermissions.EquipmentsWrite)]
    public async Task<ActionResult<EquipmentDto>> Update(
        [FromRoute] Guid id,
        [FromBody] UpdateEquipmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var cmdWithId = command with { EquipmentId = id };
        var result = await _updateHandler.HandleAsync(cmdWithId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Changes the operational status of equipment. Invalid transitions return HTTP 422.
    /// </summary>
    /// <param name="id">The unique identifier of the equipment.</param>
    /// <param name="command">The new status and optional reason.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated equipment DTO with the new status.</returns>
    [HttpPatch("{id:guid}/status")]
    [RequirePermission(FacilityPermissions.EquipmentsWrite)]
    public async Task<ActionResult<EquipmentDto>> ChangeStatus(
        [FromRoute] Guid id,
        [FromBody] ChangeEquipmentStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        var cmdWithId = command with { EquipmentId = id };
        var result = await _changeStatusHandler.HandleAsync(cmdWithId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Hard-deletes equipment. Blocked if job history exists (HTTP 409) or JobService is unreachable (HTTP 503).
    /// </summary>
    /// <param name="id">The unique identifier of the equipment to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>HTTP 204 on success.</returns>
    [HttpDelete("{id:guid}")]
    [RequirePermission(FacilityPermissions.EquipmentsManage)]
    public async Task<IActionResult> Delete(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        await _deleteHandler.HandleAsync(new DeleteEquipmentCommand(id), cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Appends a note to equipment (append-only; notes cannot be edited or deleted).
    /// </summary>
    /// <param name="id">The unique identifier of the equipment.</param>
    /// <param name="command">The note content.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created note DTO.</returns>
    [HttpPost("{id:guid}/notes")]
    [RequirePermission(FacilityPermissions.EquipmentsWrite)]
    public async Task<ActionResult<NoteDto>> AddNote(
        [FromRoute] Guid id,
        [FromBody] AddEquipmentNoteCommand command,
        CancellationToken cancellationToken = default)
    {
        var cmdWithId = command with { EquipmentId = id };
        var result = await _addNoteHandler.HandleAsync(cmdWithId, cancellationToken);
        return CreatedAtAction(nameof(GetNotes), new { id }, result);
    }

    /// <summary>
    /// Lists all notes for equipment, ordered by creation time ascending.
    /// </summary>
    /// <param name="id">The unique identifier of the equipment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of note DTOs.</returns>
    [HttpGet("{id:guid}/notes")]
    [RequirePermission(FacilityPermissions.EquipmentsRead)]
    public async Task<ActionResult<IReadOnlyList<NoteDto>>> GetNotes(
        [FromRoute] Guid id,
        CancellationToken cancellationToken = default)
    {
        var result = await _getNotesHandler.HandleAsync(
            new GetEquipmentNotesQuery(id),
            cancellationToken);
        return Ok(result);
    }
}
