using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Domain.Entities;

namespace Maliev.FacilityService.Application.Mapping;

/// <summary>
/// Manual mapping extension methods for converting note domain entities to DTOs.
/// </summary>
public static class NoteMappingExtensions
{
    /// <summary>
    /// Maps an <see cref="EquipmentNote"/> entity to a <see cref="NoteDto"/>.
    /// </summary>
    /// <param name="note">The note entity to map.</param>
    /// <returns>A <see cref="NoteDto"/> populated from the entity.</returns>
    public static NoteDto ToDto(this EquipmentNote note) =>
        new()
        {
            Id = note.Id,
            EquipmentId = note.EquipmentId,
            Content = note.Content,
            AuthorEmployeeId = note.AuthorEmployeeId,
            CreatedAt = note.CreatedAt
        };
}
