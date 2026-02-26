namespace Maliev.FacilityService.Application.UseCases.Queries.GetEquipmentLoans;

/// <summary>
/// Query to retrieve all loan records for a specific equipment.
/// </summary>
/// <param name="EquipmentId">The unique identifier of the equipment.</param>
public record GetEquipmentLoansQuery(Guid EquipmentId);
