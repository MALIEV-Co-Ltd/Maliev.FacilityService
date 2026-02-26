namespace Maliev.FacilityService.Application.UseCases.Commands.DeleteEquipment;

/// <summary>
/// Command to hard-delete equipment from the system.
/// Blocked if the equipment has job history (HTTP 409) or JobService is unreachable (HTTP 503).
/// </summary>
/// <param name="EquipmentId">ID of the equipment to delete.</param>
public record DeleteEquipmentCommand(Guid EquipmentId);
