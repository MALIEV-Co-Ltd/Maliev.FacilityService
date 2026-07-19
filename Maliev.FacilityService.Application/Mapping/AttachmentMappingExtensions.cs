using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Domain.Entities;

namespace Maliev.FacilityService.Application.Mapping;

/// <summary>
/// Manual mapping extension methods for converting attachment domain entities to DTOs.
/// </summary>
public static class AttachmentMappingExtensions
{
    /// <summary>
    /// Maps an <see cref="EquipmentAttachment"/> entity to an <see cref="AttachmentDto"/>.
    /// </summary>
    /// <param name="attachment">The attachment entity to map.</param>
    /// <returns>An <see cref="AttachmentDto"/> populated from the entity.</returns>
    public static AttachmentDto ToDto(this EquipmentAttachment attachment) =>
        new()
        {
            Id = attachment.Id,
            EquipmentId = attachment.EquipmentId,
            Name = attachment.Name,
            AttachmentType = attachment.AttachmentType,
            SerialNumber = attachment.SerialNumber,
            IsActive = attachment.IsActive,
            ConditionNotes = attachment.ConditionNotes,
            CreatedAt = attachment.CreatedAt,
            UpdatedAt = attachment.UpdatedAt
        };
}
