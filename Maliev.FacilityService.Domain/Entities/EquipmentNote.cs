namespace Maliev.FacilityService.Domain.Entities;

/// <summary>
/// Represents a note attached to an equipment record.
/// </summary>
public class EquipmentNote
{
    /// <summary>
    /// Unique identifier for the equipment note.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the associated equipment.
    /// </summary>
    public Guid EquipmentId { get; set; }

    /// <summary>
    /// Employee ID of the note author.
    /// </summary>
    public Guid AuthorEmployeeId { get; set; }

    /// <summary>
    /// Content of the note.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when the note was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
