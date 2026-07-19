using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.Mapping;

namespace Maliev.FacilityService.Application.UseCases.Queries.ListEquipments;

/// <summary>
/// Handler for the <see cref="ListEquipmentsQuery"/>.
/// Returns a paginated list of equipment with optional filters.
/// </summary>
public class ListEquipmentsQueryHandler
{
    private readonly IEquipmentRepository _equipmentRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="ListEquipmentsQueryHandler"/>.
    /// </summary>
    /// <param name="equipmentRepository">The equipment repository.</param>
    public ListEquipmentsQueryHandler(IEquipmentRepository equipmentRepository)
    {
        _equipmentRepository = equipmentRepository;
    }

    /// <summary>
    /// Handles the listing of equipment with optional filters and pagination.
    /// </summary>
    /// <param name="query">The list equipments query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paged result of equipment summary DTOs.</returns>
    public async Task<PagedResult<EquipmentSummaryDto>> HandleAsync(
        ListEquipmentsQuery query,
        CancellationToken cancellationToken = default)
    {
        var pageSize = Math.Min(query.PageSize, 100);
        var page = Math.Max(query.Page, 1);

        var filters = new EquipmentFilter
        {
            Category = query.Category,
            Status = query.Status,
            NameContains = query.Search
        };

        var pagination = new Pagination
        {
            Page = page,
            PageSize = pageSize
        };

        var (items, totalCount) = await _equipmentRepository.GetAllAsync(filters, pagination, cancellationToken);

        var dtos = items.Select(e => e.ToSummaryDto()).ToList();
        return new PagedResult<EquipmentSummaryDto>(dtos, totalCount, page, pageSize);
    }
}
