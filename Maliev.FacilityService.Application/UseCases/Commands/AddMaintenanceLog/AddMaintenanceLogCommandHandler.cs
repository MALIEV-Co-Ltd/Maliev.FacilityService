using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.Mapping;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Exceptions;

namespace Maliev.FacilityService.Application.UseCases.Commands.AddMaintenanceLog;

/// <summary>
/// Handler for the <see cref="AddMaintenanceLogCommand"/>.
/// Appends a maintenance log entry and optionally updates next service date.
/// </summary>
public class AddMaintenanceLogCommandHandler
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IMaintenanceLogRepository _maintenanceLogRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="AddMaintenanceLogCommandHandler"/>.
    /// </summary>
    /// <param name="equipmentRepository">The equipment repository.</param>
    /// <param name="maintenanceLogRepository">The maintenance log repository.</param>
    public AddMaintenanceLogCommandHandler(
        IEquipmentRepository equipmentRepository,
        IMaintenanceLogRepository maintenanceLogRepository)
    {
        _equipmentRepository = equipmentRepository;
        _maintenanceLogRepository = maintenanceLogRepository;
    }

    /// <summary>
    /// Handles the addition of a maintenance log entry.
    /// </summary>
    /// <param name="command">The add maintenance log command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created maintenance log DTO.</returns>
    /// <exception cref="EquipmentNotFoundException">Thrown when the equipment is not found.</exception>
    public async Task<MaintenanceLogDto> HandleAsync(
        AddMaintenanceLogCommand command,
        CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(command.EquipmentId, cancellationToken)
            ?? throw new EquipmentNotFoundException(command.EquipmentId);

        // Update equipment's next service due date if provided
        if (command.NextServiceDueDate.HasValue)
        {
            equipment.NextServiceDueDate = command.NextServiceDueDate;
            equipment.UpdatedAt = DateTime.UtcNow;
            await _equipmentRepository.UpdateAsync(equipment, cancellationToken);
        }

        var log = new EquipmentMaintenanceLog
        {
            Id = Guid.NewGuid(),
            EquipmentId = command.EquipmentId,
            Type = command.Type,
            Description = command.Description,
            OccurredAt = command.OccurredAt,
            LoggedByEmployeeId = command.LoggedByEmployeeId,
            VendorName = command.VendorName,
            CostTHB = command.CostTHB,
            NextServiceDueDate = command.NextServiceDueDate,
            CreatedAt = DateTime.UtcNow
        };

        var saved = await _maintenanceLogRepository.AddAsync(log, cancellationToken);
        return saved.ToDto();
    }
}
