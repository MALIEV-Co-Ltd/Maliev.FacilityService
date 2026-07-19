using Maliev.FacilityService.Domain.Entities;

namespace Maliev.FacilityService.Application.Interfaces;

/// <summary>
/// Repository interface for equipment loan operations.
/// </summary>
public interface ILoanRepository
{
    /// <summary>
    /// Retrieves a loan by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the loan.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The loan entity if found, otherwise null.</returns>
    Task<EquipmentLoan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all loans associated with an equipment.
    /// </summary>
    /// <param name="equipmentId">The unique identifier of the equipment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of loan entities for the equipment.</returns>
    Task<IReadOnlyList<EquipmentLoan>> GetByEquipmentIdAsync(
        Guid equipmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the active loan for an equipment (if any).
    /// </summary>
    /// <param name="equipmentId">The unique identifier of the equipment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The active loan entity if found, otherwise null.</returns>
    Task<EquipmentLoan?> GetActiveLoanByEquipmentIdAsync(
        Guid equipmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new loan to the repository.
    /// </summary>
    /// <param name="entity">The loan entity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The added loan entity.</returns>
    Task<EquipmentLoan> AddAsync(EquipmentLoan entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing loan.
    /// </summary>
    /// <param name="entity">The loan entity to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated loan entity.</returns>
    Task<EquipmentLoan> UpdateAsync(EquipmentLoan entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing loan and enforces optimistic concurrency using the provided xmin row version.
    /// The row version is set as the OriginalValue so EF Core includes it in the WHERE clause.
    /// </summary>
    /// <param name="entity">The loan entity to update.</param>
    /// <param name="rowVersion">The xmin value the client last saw.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated loan entity.</returns>
    Task<EquipmentLoan> UpdateAsync(EquipmentLoan entity, uint rowVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether there is any active loan (Active or PendingApproval) for the specified equipment.
    /// </summary>
    /// <param name="equipmentId">The unique identifier of the equipment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if an active or pending loan exists; otherwise false.</returns>
    Task<bool> HasActiveLoanAsync(Guid equipmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the xmin OriginalValue on the tracked entity so that EF Core enforces
    /// the client-supplied row version on the next SaveChanges call.
    /// Must be called after loading the entity and before calling UpdateAsync.
    /// </summary>
    /// <param name="entity">The tracked loan entity.</param>
    /// <param name="rowVersion">The xmin value the client last saw.</param>
    void SetXminOriginalValue(EquipmentLoan entity, uint rowVersion);
}
