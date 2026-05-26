using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Application.Interfaces;

/// <summary>
/// Repository interface for equipment CRUD and query operations.
/// </summary>
public interface IEquipmentRepository
{
    /// <summary>
    /// Retrieves an equipment by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the equipment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The equipment entity if found, otherwise null.</returns>
    Task<Equipment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an equipment by its unique identifier with its xmin row version.
    /// </summary>
    /// <param name="id">The unique identifier of the equipment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The equipment entity and xmin row version if found, otherwise null.</returns>
    Task<(Equipment Equipment, uint RowVersion)?> GetByIdWithRowVersionAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all equipment with optional filtering and pagination.
    /// </summary>
    /// <param name="filters">Optional filter criteria.</param>
    /// <param name="pagination">Optional pagination parameters.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of equipment entities.</returns>
    Task<(IReadOnlyList<Equipment> Items, int TotalCount)> GetAllAsync(
        EquipmentFilter? filters = null,
        Pagination? pagination = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active equipment by category.
    /// </summary>
    /// <param name="category">The equipment category.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of active equipment in the specified category.</returns>
    Task<IReadOnlyList<Equipment>> GetActiveByCategoryAsync(
        EquipmentCategory category,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all active equipment across multiple categories in a single query.
    /// </summary>
    /// <param name="categories">The set of equipment categories to include.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of active equipment in any of the specified categories.</returns>
    Task<IReadOnlyList<Equipment>> GetActiveByMultipleCategoriesAsync(
        IReadOnlySet<EquipmentCategory> categories,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an equipment with the given name exists.
    /// </summary>
    /// <param name="name">The name to check.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if an equipment with the name exists, otherwise false.</returns>
    Task<bool> ExistsByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new equipment to the repository.
    /// </summary>
    /// <param name="entity">The equipment entity to add.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The added equipment entity.</returns>
    Task<Equipment> AddAsync(Equipment entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing equipment.
    /// </summary>
    /// <param name="entity">The equipment entity to update.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated equipment entity.</returns>
    Task<Equipment> UpdateAsync(Equipment entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing equipment and enforces optimistic concurrency using the provided xmin row version.
    /// The row version is set as the OriginalValue so EF Core includes it in the WHERE clause.
    /// </summary>
    /// <param name="entity">The equipment entity to update.</param>
    /// <param name="rowVersion">The xmin value the client last saw.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated equipment entity.</returns>
    Task<Equipment> UpdateAsync(Equipment entity, uint rowVersion, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an equipment by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the equipment to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the equipment was deleted, otherwise false.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sets the xmin OriginalValue on the tracked entity so that EF Core enforces
    /// the client-supplied row version on the next SaveChanges call.
    /// Must be called after loading the entity and before calling UpdateAsync.
    /// </summary>
    /// <param name="entity">The tracked equipment entity.</param>
    /// <param name="rowVersion">The xmin value the client last saw.</param>
    void SetXminOriginalValue(Equipment entity, uint rowVersion);
}

/// <summary>
/// Filter criteria for equipment queries.
/// </summary>
public class EquipmentFilter
{
    /// <summary>
    /// Filter by equipment category.
    /// </summary>
    public EquipmentCategory? Category { get; set; }

    /// <summary>
    /// Filter by equipment status.
    /// </summary>
    public EquipmentStatus? Status { get; set; }

    /// <summary>
    /// Filter by name (partial match).
    /// </summary>
    public string? NameContains { get; set; }

    /// <summary>
    /// Filter by brand.
    /// </summary>
    public string? Brand { get; set; }
}

/// <summary>
/// Pagination parameters for queries.
/// </summary>
public class Pagination
{
    /// <summary>
    /// Page number (1-based).
    /// </summary>
    public int Page { get; set; } = 1;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; set; } = 20;
}
