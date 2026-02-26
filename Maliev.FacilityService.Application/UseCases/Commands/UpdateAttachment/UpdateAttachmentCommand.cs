namespace Maliev.FacilityService.Application.UseCases.Commands.UpdateAttachment;

/// <summary>
/// Command to update an existing equipment attachment.
/// </summary>
/// <param name="EquipmentId">ID of the equipment. Set by the controller from the route.</param>
/// <param name="AttachmentId">ID of the attachment to update. Set by the controller from the route.</param>
/// <param name="Name">New name of the attachment.</param>
/// <param name="SerialNumber">New serial number.</param>
/// <param name="IsActive">Whether the attachment is active.</param>
/// <param name="ConditionNotes">Updated condition notes.</param>
/// <param name="RowVersion">xmin row version for optimistic concurrency.</param>
public record UpdateAttachmentCommand(
    Guid EquipmentId,
    Guid AttachmentId,
    string Name,
    string? SerialNumber,
    bool IsActive,
    string? ConditionNotes,
    uint RowVersion);
