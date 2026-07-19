using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.Mapping;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Exceptions;

namespace Maliev.FacilityService.Application.UseCases.Commands.UpdateEquipment;

/// <summary>
/// Handler for the <see cref="UpdateEquipmentCommand"/>.
/// Updates an existing equipment record.
/// </summary>
public class UpdateEquipmentCommandHandler
{
    private readonly IEquipmentRepository _equipmentRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="UpdateEquipmentCommandHandler"/>.
    /// </summary>
    /// <param name="equipmentRepository">The equipment repository.</param>
    public UpdateEquipmentCommandHandler(IEquipmentRepository equipmentRepository)
    {
        _equipmentRepository = equipmentRepository;
    }

    /// <summary>
    /// Handles the update of equipment details.
    /// </summary>
    /// <param name="command">The update equipment command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated equipment DTO.</returns>
    /// <exception cref="EquipmentNotFoundException">Thrown when the equipment is not found.</exception>
    public async Task<EquipmentDto> HandleAsync(
        UpdateEquipmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(command.EquipmentId, cancellationToken)
            ?? throw new EquipmentNotFoundException(command.EquipmentId);

        equipment.Name = command.Name;
        equipment.Brand = command.Brand;
        equipment.ModelName = command.ModelName;
        equipment.ManufacturerSerialNumber = command.ManufacturerSerialNumber;
        equipment.SubCategory = command.SubCategory;
        equipment.PurchaseDate = command.PurchaseDate;
        equipment.PurchasePriceTHB = command.PurchasePriceTHB;
        equipment.WarrantyExpiryDate = command.WarrantyExpiryDate;
        equipment.NextServiceDueDate = command.NextServiceDueDate;
        equipment.UpdatedAt = DateTime.UtcNow;

        if (command.Spec is not null)
        {
            switch (equipment)
            {
                case FdmPrinterEquipment fdm:
                    fdm.ApplySpec(command.Spec);
                    break;
                case SlaPrinterEquipment sla:
                    sla.ApplySpec(command.Spec);
                    break;
                case CncMachineEquipment cnc:
                    cnc.ApplySpec(command.Spec);
                    break;
                case Scanner3DEquipment scanner:
                    scanner.ApplySpec(command.Spec);
                    break;
                case InjectionMoldingEquipment im:
                    im.ApplySpec(command.Spec);
                    break;
            }
        }

        var updated = await _equipmentRepository.UpdateAsync(equipment, command.RowVersion, cancellationToken);
        return updated.ToDto();
    }
}
