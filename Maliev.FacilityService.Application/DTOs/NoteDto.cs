namespace Maliev.FacilityService.Application.DTOs;

/// <summary>
/// DTO representing an equipment note.
/// </summary>
public record NoteDto
{
    /// <summary>
    /// Unique identifier of the note.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// ID of the associated equipment.
    /// </summary>
    public Guid EquipmentId { get; init; }

    /// <summary>
    /// Content of the note.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Employee ID of the note author.
    /// </summary>
    public Guid AuthorEmployeeId { get; init; }

    /// <summary>
    /// Timestamp when the note was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
