using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Application.UseCases.Queries.GetActiveEquipmentsByCategory;

/// <summary>
/// Query to retrieve active manufacturing equipment, optionally filtered by category.
/// Used by PricingService and JobService for machine availability checks.
/// </summary>
/// <param name="Category">Optional category filter. Null returns all active equipment.</param>
public record GetActiveEquipmentsByCategoryQuery(EquipmentCategory? Category);
