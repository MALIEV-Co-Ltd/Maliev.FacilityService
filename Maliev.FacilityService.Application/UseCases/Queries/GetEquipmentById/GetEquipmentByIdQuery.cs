namespace Maliev.FacilityService.Application.UseCases.Queries.GetEquipmentById;

/// <summary>
/// Query to retrieve a single equipment record by its unique identifier.
/// </summary>
/// <param name="EquipmentId">The unique identifier of the equipment.</param>
public record GetEquipmentByIdQuery(Guid EquipmentId);
