using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.Mapping;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Application.UseCases.Queries.GetActiveEquipmentsByCategory;

/// <summary>
/// Handler for the <see cref="GetActiveEquipmentsByCategoryQuery"/>.
/// Returns active manufacturing equipment with pricing data.
/// </summary>
public class GetActiveEquipmentsByCategoryQueryHandler
{
    private static readonly IReadOnlySet<EquipmentCategory> ManufacturingCategories = new HashSet<EquipmentCategory>
    {
        EquipmentCategory.FdmPrinter,
        EquipmentCategory.SlaPrinter,
        EquipmentCategory.CncMachine,
        EquipmentCategory.Scanner3D,
        EquipmentCategory.InjectionMolding
    };

    private readonly IEquipmentRepository _equipmentRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="GetActiveEquipmentsByCategoryQueryHandler"/>.
    /// </summary>
    /// <param name="equipmentRepository">The equipment repository.</param>
    public GetActiveEquipmentsByCategoryQueryHandler(IEquipmentRepository equipmentRepository)
    {
        _equipmentRepository = equipmentRepository;
    }

    /// <summary>
    /// Handles the retrieval of active equipment by category.
    /// Only manufacturing equipment is returned as it has hourly rate and setup fee data.
    /// </summary>
    /// <param name="query">The get active equipment query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of active manufacturing equipment DTOs.</returns>
    public async Task<IReadOnlyList<ActiveEquipmentDto>> HandleAsync(
        GetActiveEquipmentsByCategoryQuery query,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Equipment> items;

        if (query.Category.HasValue)
        {
            items = await _equipmentRepository.GetActiveByCategoryAsync(query.Category.Value, cancellationToken);
        }
        else
        {
            // Return all active manufacturing equipment in a single query
            items = await _equipmentRepository.GetActiveByMultipleCategoriesAsync(ManufacturingCategories, cancellationToken);
        }

        return items
            .Where(e => e is ManufacturingEquipment)
            .Select(e => e.ToActiveDto())
            .ToList();
    }
}
