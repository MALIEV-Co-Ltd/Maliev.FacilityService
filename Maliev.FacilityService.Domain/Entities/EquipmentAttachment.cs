using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Domain.Entities;

/// <summary>
/// Represents an attachment (tool, fixture, etc.) associated with equipment.
/// </summary>
public class EquipmentAttachment
{
    /// <summary>
    /// Unique identifier for the attachment.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the associated equipment.
    /// </summary>
    public Guid EquipmentId { get; set; }

    /// <summary>
    /// Name of the attachment.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Type of attachment.
    /// </summary>
    public AttachmentType AttachmentType { get; set; }

    /// <summary>
    /// Serial number of the attachment.
    /// </summary>
    public string? SerialNumber { get; set; }

    /// <summary>
    /// Indicates whether the attachment is currently active.
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// Notes on the condition of the attachment.
    /// </summary>
    public string? ConditionNotes { get; set; }

    /// <summary>
    /// Timestamp when the attachment record was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the attachment record was last updated (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
