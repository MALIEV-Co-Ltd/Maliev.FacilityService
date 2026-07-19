using Maliev.FacilityService.Domain.Entities;

namespace Maliev.FacilityService.Application.Interfaces;

/// <summary>
/// Repository interface for equipment note operations.
/// </summary>
public interface IEquipmentNoteRepository
{
    /// <summary>
    /// Retrieves all notes for a given equipment.
    /// </summary>
    /// <param name="equipmentId">The unique identifier of the equipment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of note entities for the equipment.</returns>
    Task<IReadOnlyList<EquipmentNote>> GetByEquipmentIdAsync(
        Guid equipmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new note to the repository.
    /// </summary>
    /// <param name="entity">The note entity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The added note entity.</returns>
    Task<EquipmentNote> AddAsync(EquipmentNote entity, CancellationToken cancellationToken = default);
}
