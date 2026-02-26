using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.Mapping;
using Maliev.FacilityService.Domain.Exceptions;

namespace Maliev.FacilityService.Application.UseCases.Queries.GetEquipmentById;

/// <summary>
/// Handler for the <see cref="GetEquipmentByIdQuery"/>.
/// Retrieves a single equipment record with full detail including spec data.
/// </summary>
public class GetEquipmentByIdQueryHandler
{
    private readonly IEquipmentRepository _equipmentRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="GetEquipmentByIdQueryHandler"/>.
    /// </summary>
    /// <param name="equipmentRepository">The equipment repository.</param>
    public GetEquipmentByIdQueryHandler(IEquipmentRepository equipmentRepository)
    {
        _equipmentRepository = equipmentRepository;
    }

    /// <summary>
    /// Handles the retrieval of a single equipment by ID.
    /// </summary>
    /// <param name="query">The get by ID query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The equipment DTO with full detail.</returns>
    /// <exception cref="EquipmentNotFoundException">Thrown when the equipment is not found.</exception>
    public async Task<EquipmentDto> HandleAsync(
        GetEquipmentByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(query.EquipmentId, cancellationToken)
            ?? throw new EquipmentNotFoundException(query.EquipmentId);

        return equipment.ToDto();
    }
}
