using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.UseCases.Commands.AddAttachment;
using Maliev.FacilityService.Application.UseCases.Commands.UpdateAttachment;
using Maliev.FacilityService.Application.UseCases.Queries.GetAttachments;
using Maliev.FacilityService.Domain.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.FacilityService.Api.Controllers;

/// <summary>
/// Manages CNC machine attachments (tools, fixtures, collets, etc.) for equipment.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("facility/v{version:apiVersion}/equipments/{equipmentId:guid}/attachments")]
public class AttachmentsController : ControllerBase
{
    private readonly AddAttachmentCommandHandler _addHandler;
    private readonly UpdateAttachmentCommandHandler _updateHandler;
    private readonly GetAttachmentsQueryHandler _getHandler;

    /// <summary>
    /// Initializes a new instance of <see cref="AttachmentsController"/>.
    /// </summary>
    /// <param name="addHandler">Handler for adding attachments.</param>
    /// <param name="updateHandler">Handler for updating attachments.</param>
    /// <param name="getHandler">Handler for retrieving attachments.</param>
    public AttachmentsController(
        AddAttachmentCommandHandler addHandler,
        UpdateAttachmentCommandHandler updateHandler,
        GetAttachmentsQueryHandler getHandler)
    {
        _addHandler = addHandler;
        _updateHandler = updateHandler;
        _getHandler = getHandler;
    }

    /// <summary>
    /// Retrieves all attachments for a piece of equipment.
    /// </summary>
    /// <param name="equipmentId">The unique identifier of the equipment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of attachment DTOs.</returns>
    [HttpGet]
    [RequirePermission(FacilityPermissions.AttachmentsRead)]
    public async Task<ActionResult<IReadOnlyList<AttachmentDto>>> GetAttachments(
        [FromRoute] Guid equipmentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _getHandler.HandleAsync(
            new GetAttachmentsQuery(equipmentId),
            cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Adds a new CNC attachment to equipment.
    /// </summary>
    /// <param name="equipmentId">The unique identifier of the equipment.</param>
    /// <param name="command">The attachment details including type and optional serial number.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created attachment DTO.</returns>
    [HttpPost]
    [RequirePermission(FacilityPermissions.AttachmentsWrite)]
    public async Task<ActionResult<AttachmentDto>> AddAttachment(
        [FromRoute] Guid equipmentId,
        [FromBody] AddAttachmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var cmdWithId = command with { EquipmentId = equipmentId };
        var result = await _addHandler.HandleAsync(cmdWithId, cancellationToken);
        var version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1";
        return Created($"facility/v{version}/equipments/{equipmentId}/attachments/{result.Id}", result);
    }

    /// <summary>
    /// Updates an existing attachment's details or active status.
    /// Requires the current xmin row version for concurrency control.
    /// </summary>
    /// <param name="equipmentId">The unique identifier of the equipment.</param>
    /// <param name="attachmentId">The unique identifier of the attachment to update.</param>
    /// <param name="command">The update details.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated attachment DTO.</returns>
    [HttpPut("{attachmentId:guid}")]
    [RequirePermission(FacilityPermissions.AttachmentsWrite)]
    public async Task<ActionResult<AttachmentDto>> UpdateAttachment(
        [FromRoute] Guid equipmentId,
        [FromRoute] Guid attachmentId,
        [FromBody] UpdateAttachmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var cmdWithIds = command with { EquipmentId = equipmentId, AttachmentId = attachmentId };
        var result = await _updateHandler.HandleAsync(cmdWithIds, cancellationToken);
        return Ok(result);
    }
}
