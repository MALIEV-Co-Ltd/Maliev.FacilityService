namespace Maliev.FacilityService.Application.DTOs;

/// <summary>
/// Generic wrapper for paginated query results.
/// </summary>
/// <typeparam name="T">The type of items in the result set.</typeparam>
/// <param name="Items">The items on the current page.</param>
/// <param name="TotalCount">Total number of items across all pages.</param>
/// <param name="Page">The current page number (1-based).</param>
/// <param name="PageSize">The number of items per page.</param>
public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
