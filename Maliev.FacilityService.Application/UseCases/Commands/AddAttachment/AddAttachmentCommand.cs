using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Application.UseCases.Commands.AddAttachment;

/// <summary>
/// Command to add a CNC attachment to equipment.
/// </summary>
/// <param name="EquipmentId">ID of the equipment. Set by the controller from the route.</param>
/// <param name="Name">Name of the attachment.</param>
/// <param name="AttachmentType">Type of attachment.</param>
/// <param name="SerialNumber">Serial number of the attachment.</param>
/// <param name="ConditionNotes">Notes on the condition of the attachment.</param>
public record AddAttachmentCommand(
    Guid EquipmentId,
    string Name,
    AttachmentType AttachmentType,
    string? SerialNumber,
    string? ConditionNotes);
