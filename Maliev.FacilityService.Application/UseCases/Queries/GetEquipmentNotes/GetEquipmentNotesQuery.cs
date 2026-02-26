namespace Maliev.FacilityService.Application.UseCases.Queries.GetEquipmentNotes;

/// <summary>
/// Query to retrieve all notes for a specific equipment record.
/// </summary>
/// <param name="EquipmentId">The unique identifier of the equipment.</param>
public record GetEquipmentNotesQuery(Guid EquipmentId);
