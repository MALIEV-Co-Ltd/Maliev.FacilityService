using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Application.UseCases.Commands.ChangeEquipmentStatus;

/// <summary>
/// Command to change the operational status of equipment.
/// </summary>
/// <param name="EquipmentId">ID of the equipment. Set by the controller from the route.</param>
/// <param name="NewStatus">The new status to transition to.</param>
/// <param name="Reason">Optional reason for the status change.</param>
/// <param name="RowVersion">xmin row version for optimistic concurrency.</param>
public record ChangeEquipmentStatusCommand(
    Guid EquipmentId,
    EquipmentStatus NewStatus,
    string? Reason,
    uint RowVersion);
