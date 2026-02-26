namespace Maliev.FacilityService.Application.UseCases.Commands.AddEquipmentNote;

/// <summary>
/// Command to append a note to an equipment record (append-only).
/// </summary>
/// <param name="EquipmentId">ID of the equipment. Set by the controller from the route.</param>
/// <param name="Content">Content of the note.</param>
/// <param name="AuthorEmployeeId">ID of the employee authoring the note.</param>
public record AddEquipmentNoteCommand(Guid EquipmentId, string Content, Guid AuthorEmployeeId);
