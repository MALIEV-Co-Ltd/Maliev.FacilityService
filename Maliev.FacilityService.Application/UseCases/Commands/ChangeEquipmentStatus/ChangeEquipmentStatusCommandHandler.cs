using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.Mapping;
using Maliev.FacilityService.Domain.Exceptions;

namespace Maliev.FacilityService.Application.UseCases.Commands.ChangeEquipmentStatus;

/// <summary>
/// Handler for the <see cref="ChangeEquipmentStatusCommand"/>.
/// Validates and applies the status transition, publishing a domain event.
/// </summary>
public class ChangeEquipmentStatusCommandHandler
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of <see cref="ChangeEquipmentStatusCommandHandler"/>.
    /// </summary>
    /// <param name="equipmentRepository">The equipment repository.</param>
    /// <param name="eventPublisher">The event publisher.</param>
    public ChangeEquipmentStatusCommandHandler(
        IEquipmentRepository equipmentRepository,
        IEventPublisher eventPublisher)
    {
        _equipmentRepository = equipmentRepository;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Handles the status change for equipment.
    /// </summary>
    /// <param name="command">The change status command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated equipment DTO with the new status.</returns>
    /// <exception cref="EquipmentNotFoundException">Thrown when the equipment is not found.</exception>
    /// <exception cref="InvalidStatusTransitionException">Thrown when the status transition is invalid.</exception>
    public async Task<EquipmentDto> HandleAsync(
        ChangeEquipmentStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(command.EquipmentId, cancellationToken)
            ?? throw new EquipmentNotFoundException(command.EquipmentId);

        var previousStatus = equipment.Status;
        equipment.TransitionTo(command.NewStatus);
        equipment.UpdatedAt = DateTime.UtcNow;

        var updated = await _equipmentRepository.UpdateAsync(equipment, cancellationToken);

        await _eventPublisher.PublishEquipmentStatusChangedAsync(
            equipment.Id,
            equipment.AssetCode,
            equipment.Name,
            equipment.Category.ToString(),
            previousStatus.ToString(),
            command.NewStatus.ToString(),
            cancellationToken);

        return updated.ToDto();
    }
}
