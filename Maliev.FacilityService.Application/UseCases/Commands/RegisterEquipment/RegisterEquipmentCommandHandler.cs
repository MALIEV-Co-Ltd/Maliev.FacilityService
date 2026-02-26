using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.Mapping;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Application.UseCases.Commands.RegisterEquipment;

/// <summary>
/// Handler for the <see cref="RegisterEquipmentCommand"/>.
/// Registers a new piece of equipment and generates its asset code.
/// </summary>
public class RegisterEquipmentCommandHandler
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IAssetCodeGenerator _assetCodeGenerator;

    /// <summary>
    /// Initializes a new instance of <see cref="RegisterEquipmentCommandHandler"/>.
    /// </summary>
    /// <param name="equipmentRepository">The equipment repository.</param>
    /// <param name="assetCodeGenerator">The asset code generator service.</param>
    public RegisterEquipmentCommandHandler(
        IEquipmentRepository equipmentRepository,
        IAssetCodeGenerator assetCodeGenerator)
    {
        _equipmentRepository = equipmentRepository;
        _assetCodeGenerator = assetCodeGenerator;
    }

    /// <summary>
    /// Handles the registration of new equipment.
    /// </summary>
    /// <param name="command">The register equipment command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created equipment DTO with generated asset code.</returns>
    public async Task<EquipmentDto> HandleAsync(
        RegisterEquipmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var assetCode = await _assetCodeGenerator.GenerateAssetCodeAsync(command.Category, cancellationToken);

        Equipment equipment = command.Category switch
        {
            EquipmentCategory.FdmPrinter => CreateFdmPrinter(command, assetCode, now),
            EquipmentCategory.SlaPrinter => CreateSlaPrinter(command, assetCode, now),
            EquipmentCategory.CncMachine => CreateCncMachine(command, assetCode, now),
            EquipmentCategory.Scanner3D => CreateScanner3D(command, assetCode, now),
            EquipmentCategory.InjectionMolding => CreateInjectionMolding(command, assetCode, now),
            EquipmentCategory.OfficeEquipment => CreateGeneralEquipment<OfficeEquipmentItem>(command, assetCode, now),
            EquipmentCategory.MeasuringEquipment => CreateGeneralEquipment<MeasuringEquipmentItem>(command, assetCode, now),
            EquipmentCategory.ITEquipment => CreateGeneralEquipment<ITEquipmentItem>(command, assetCode, now),
            EquipmentCategory.HandTool => CreateGeneralEquipment<HandToolItem>(command, assetCode, now),
            EquipmentCategory.Other => CreateGeneralEquipment<OtherEquipmentItem>(command, assetCode, now),
            _ => throw new InvalidOperationException($"Unknown equipment category: {command.Category}")
        };

        var saved = await _equipmentRepository.AddAsync(equipment, cancellationToken);
        return saved.ToDto();
    }

    private static FdmPrinterEquipment CreateFdmPrinter(
        RegisterEquipmentCommand command,
        string assetCode,
        DateTime now)
    {
        var entity = new FdmPrinterEquipment();
        PopulateBase(entity, command, assetCode, now);
        if (command.Spec is not null)
            entity.ApplySpec(command.Spec);
        return entity;
    }

    private static SlaPrinterEquipment CreateSlaPrinter(
        RegisterEquipmentCommand command,
        string assetCode,
        DateTime now)
    {
        var entity = new SlaPrinterEquipment();
        PopulateBase(entity, command, assetCode, now);
        if (command.Spec is not null)
            entity.ApplySpec(command.Spec);
        return entity;
    }

    private static CncMachineEquipment CreateCncMachine(
        RegisterEquipmentCommand command,
        string assetCode,
        DateTime now)
    {
        var entity = new CncMachineEquipment();
        PopulateBase(entity, command, assetCode, now);
        if (command.Spec is not null)
            entity.ApplySpec(command.Spec);
        return entity;
    }

    private static Scanner3DEquipment CreateScanner3D(
        RegisterEquipmentCommand command,
        string assetCode,
        DateTime now)
    {
        var entity = new Scanner3DEquipment();
        PopulateBase(entity, command, assetCode, now);
        if (command.Spec is not null)
            entity.ApplySpec(command.Spec);
        return entity;
    }

    private static InjectionMoldingEquipment CreateInjectionMolding(
        RegisterEquipmentCommand command,
        string assetCode,
        DateTime now)
    {
        var entity = new InjectionMoldingEquipment();
        PopulateBase(entity, command, assetCode, now);
        if (command.Spec is not null)
            entity.ApplySpec(command.Spec);
        return entity;
    }

    private static T CreateGeneralEquipment<T>(
        RegisterEquipmentCommand command,
        string assetCode,
        DateTime now)
        where T : GeneralEquipment, new()
    {
        var entity = new T();
        PopulateBase(entity, command, assetCode, now);
        return entity;
    }

    private static void PopulateBase(
        Equipment equipment,
        RegisterEquipmentCommand command,
        string assetCode,
        DateTime now)
    {
        equipment.Id = Guid.NewGuid();
        equipment.AssetCode = assetCode;
        equipment.Name = command.Name;
        equipment.Brand = command.Brand;
        equipment.ModelName = command.ModelName;
        equipment.ManufacturerSerialNumber = command.ManufacturerSerialNumber;
        equipment.Category = command.Category;
        equipment.SubCategory = command.SubCategory;
        equipment.Status = EquipmentStatus.Active;
        equipment.PurchaseDate = command.PurchaseDate;
        equipment.PurchasePriceTHB = command.PurchasePriceTHB;
        equipment.WarrantyExpiryDate = command.WarrantyExpiryDate;
        equipment.NextServiceDueDate = command.NextServiceDueDate;
        equipment.CreatedAt = now;
        equipment.UpdatedAt = now;
    }
}
