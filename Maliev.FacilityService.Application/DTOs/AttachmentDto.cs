using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Application.DTOs;

/// <summary>
/// DTO representing an equipment attachment record.
/// </summary>
public record AttachmentDto
{
    /// <summary>
    /// Unique identifier of the attachment.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// ID of the associated equipment.
    /// </summary>
    public Guid EquipmentId { get; init; }

    /// <summary>
    /// Name of the attachment.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Type of attachment.
    /// </summary>
    public AttachmentType AttachmentType { get; init; }

    /// <summary>
    /// Serial number of the attachment.
    /// </summary>
    public string? SerialNumber { get; init; }

    /// <summary>
    /// Indicates whether the attachment is currently active.
    /// </summary>
    public bool IsActive { get; init; }

    /// <summary>
    /// Notes on the condition of the attachment.
    /// </summary>
    public string? ConditionNotes { get; init; }

    /// <summary>
    /// Timestamp when the attachment record was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Timestamp when the attachment record was last updated (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}
