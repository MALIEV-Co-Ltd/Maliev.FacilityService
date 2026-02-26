using Maliev.FacilityService.Domain.Entities;

namespace Maliev.FacilityService.Application.Interfaces;

/// <summary>
/// Repository interface for equipment attachment operations.
/// </summary>
public interface IAttachmentRepository
{
    /// <summary>
    /// Retrieves all attachments for an equipment.
    /// </summary>
    /// <param name="equipmentId">The unique identifier of the equipment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of attachment entities for the equipment.</returns>
    Task<IReadOnlyList<EquipmentAttachment>> GetByEquipmentIdAsync(
        Guid equipmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active attachments for an equipment.
    /// </summary>
    /// <param name="equipmentId">The unique identifier of the equipment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of active attachment entities for the equipment.</returns>
    Task<IReadOnlyList<EquipmentAttachment>> GetActiveByEquipmentIdAsync(
        Guid equipmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new attachment to the repository.
    /// </summary>
    /// <param name="entity">The attachment entity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The added attachment entity.</returns>
    Task<EquipmentAttachment> AddAsync(
        EquipmentAttachment entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing attachment.
    /// </summary>
    /// <param name="entity">The attachment entity to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated attachment entity.</returns>
    Task<EquipmentAttachment> UpdateAsync(
        EquipmentAttachment entity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an attachment by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the attachment to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the attachment was deleted, otherwise false.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
