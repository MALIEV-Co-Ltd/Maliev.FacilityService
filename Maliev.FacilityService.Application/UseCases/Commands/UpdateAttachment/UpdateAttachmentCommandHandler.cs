using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.Mapping;

namespace Maliev.FacilityService.Application.UseCases.Commands.UpdateAttachment;

/// <summary>
/// Handler for the <see cref="UpdateAttachmentCommand"/>.
/// Updates an existing equipment attachment.
/// </summary>
public class UpdateAttachmentCommandHandler
{
    private readonly IAttachmentRepository _attachmentRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="UpdateAttachmentCommandHandler"/>.
    /// </summary>
    /// <param name="attachmentRepository">The attachment repository.</param>
    public UpdateAttachmentCommandHandler(IAttachmentRepository attachmentRepository)
    {
        _attachmentRepository = attachmentRepository;
    }

    /// <summary>
    /// Handles the update of an attachment.
    /// </summary>
    /// <param name="command">The update attachment command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated attachment DTO.</returns>
    public async Task<AttachmentDto> HandleAsync(
        UpdateAttachmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var attachments = await _attachmentRepository.GetByEquipmentIdAsync(command.EquipmentId, cancellationToken);
        var attachment = attachments.FirstOrDefault(a => a.Id == command.AttachmentId)
            ?? throw new KeyNotFoundException($"Attachment '{command.AttachmentId}' not found for equipment '{command.EquipmentId}'.");

        attachment.Name = command.Name;
        attachment.SerialNumber = command.SerialNumber;
        attachment.IsActive = command.IsActive;
        attachment.ConditionNotes = command.ConditionNotes;
        attachment.UpdatedAt = DateTime.UtcNow;

        var updated = await _attachmentRepository.UpdateAsync(attachment, command.RowVersion, cancellationToken);
        return updated.ToDto();
    }
}
