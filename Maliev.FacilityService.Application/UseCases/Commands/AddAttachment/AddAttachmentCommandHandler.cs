using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.Mapping;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Exceptions;

namespace Maliev.FacilityService.Application.UseCases.Commands.AddAttachment;

/// <summary>
/// Handler for the <see cref="AddAttachmentCommand"/>.
/// Adds a CNC attachment to equipment. Only CNC machines support attachments.
/// </summary>
public class AddAttachmentCommandHandler
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IAttachmentRepository _attachmentRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="AddAttachmentCommandHandler"/>.
    /// </summary>
    /// <param name="equipmentRepository">The equipment repository.</param>
    /// <param name="attachmentRepository">The attachment repository.</param>
    public AddAttachmentCommandHandler(
        IEquipmentRepository equipmentRepository,
        IAttachmentRepository attachmentRepository)
    {
        _equipmentRepository = equipmentRepository;
        _attachmentRepository = attachmentRepository;
    }

    /// <summary>
    /// Handles the addition of an attachment to equipment.
    /// </summary>
    /// <param name="command">The add attachment command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created attachment DTO.</returns>
    /// <exception cref="EquipmentNotFoundException">Thrown when the equipment is not found.</exception>
    /// <exception cref="AttachmentNotAllowedException">Thrown when the equipment is not a CNC machine.</exception>
    public async Task<AttachmentDto> HandleAsync(
        AddAttachmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(command.EquipmentId, cancellationToken)
            ?? throw new EquipmentNotFoundException(command.EquipmentId);

        if (equipment is not CncMachineEquipment)
            throw new AttachmentNotAllowedException(command.EquipmentId, equipment.Category.ToString());

        var now = DateTime.UtcNow;
        var attachment = new EquipmentAttachment
        {
            Id = Guid.NewGuid(),
            EquipmentId = command.EquipmentId,
            Name = command.Name,
            AttachmentType = command.AttachmentType,
            SerialNumber = command.SerialNumber,
            ConditionNotes = command.ConditionNotes,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var saved = await _attachmentRepository.AddAsync(attachment, cancellationToken);
        return saved.ToDto();
    }
}
