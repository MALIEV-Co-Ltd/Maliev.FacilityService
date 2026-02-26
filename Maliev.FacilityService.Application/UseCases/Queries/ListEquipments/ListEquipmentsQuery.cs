using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Application.UseCases.Queries.ListEquipments;

/// <summary>
/// Query to list equipment with optional filters and pagination.
/// </summary>
/// <param name="Category">Optional category filter.</param>
/// <param name="Status">Optional status filter.</param>
/// <param name="Search">Optional search term (name, brand, asset code).</param>
/// <param name="Page">Page number (1-based).</param>
/// <param name="PageSize">Number of items per page.</param>
public record ListEquipmentsQuery(
    EquipmentCategory? Category,
    EquipmentStatus? Status,
    string? Search,
    int Page,
    int PageSize);
