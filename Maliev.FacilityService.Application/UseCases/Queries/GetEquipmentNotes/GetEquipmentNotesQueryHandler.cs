using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.Mapping;

namespace Maliev.FacilityService.Application.UseCases.Queries.GetEquipmentNotes;

/// <summary>
/// Handler for the <see cref="GetEquipmentNotesQuery"/>.
/// Returns all notes for the specified equipment, ordered by creation time.
/// </summary>
public class GetEquipmentNotesQueryHandler
{
    private readonly IEquipmentNoteRepository _noteRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="GetEquipmentNotesQueryHandler"/>.
    /// </summary>
    /// <param name="noteRepository">The equipment note repository.</param>
    public GetEquipmentNotesQueryHandler(IEquipmentNoteRepository noteRepository)
    {
        _noteRepository = noteRepository;
    }

    /// <summary>
    /// Handles retrieval of equipment notes.
    /// </summary>
    /// <param name="query">The get notes query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of note DTOs for the equipment.</returns>
    public async Task<IReadOnlyList<NoteDto>> HandleAsync(
        GetEquipmentNotesQuery query,
        CancellationToken cancellationToken = default)
    {
        var notes = await _noteRepository.GetByEquipmentIdAsync(query.EquipmentId, cancellationToken);
        return notes.Select(n => n.ToDto()).ToList();
    }
}
