using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Application.Mapping;

/// <summary>
/// Manual mapping extension methods for converting Equipment domain entities to DTOs.
/// </summary>
public static class EquipmentMappingExtensions
{
    /// <summary>
    /// Maps an <see cref="Equipment"/> entity to an <see cref="EquipmentSummaryDto"/>.
    /// </summary>
    /// <param name="equipment">The equipment entity to map.</param>
    /// <returns>An <see cref="EquipmentSummaryDto"/> populated from the entity.</returns>
    public static EquipmentSummaryDto ToSummaryDto(this Equipment equipment) =>
        new()
        {
            Id = equipment.Id,
            AssetCode = equipment.AssetCode,
            Name = equipment.Name,
            Brand = equipment.Brand,
            ModelName = equipment.ModelName,
            Category = equipment.Category,
            Status = equipment.Status,
            PurchasePriceTHB = equipment.PurchasePriceTHB,
            NextServiceDueDate = equipment.NextServiceDueDate,
            UpdatedAt = equipment.UpdatedAt
        };

    /// <summary>
    /// Maps an <see cref="Equipment"/> entity to an <see cref="EquipmentDto"/> with full detail and spec data.
    /// </summary>
    /// <param name="equipment">The equipment entity to map.</param>
    /// <returns>An <see cref="EquipmentDto"/> populated from the entity.</returns>
    public static EquipmentDto ToDto(this Equipment equipment) =>
        new()
        {
            Id = equipment.Id,
            AssetCode = equipment.AssetCode,
            Name = equipment.Name,
            Brand = equipment.Brand,
            ModelName = equipment.ModelName,
            Category = equipment.Category,
            Status = equipment.Status,
            PurchasePriceTHB = equipment.PurchasePriceTHB,
            UpdatedAt = equipment.UpdatedAt,
            ManufacturerSerialNumber = equipment.ManufacturerSerialNumber,
            SubCategory = equipment.SubCategory,
            PurchaseDate = equipment.PurchaseDate,
            WarrantyExpiryDate = equipment.WarrantyExpiryDate,
            NextServiceDueDate = equipment.NextServiceDueDate,
            CreatedAt = equipment.CreatedAt,
            Spec = equipment.ToSpecDictionary()
        };

    /// <summary>
    /// Maps an <see cref="Equipment"/> entity to an <see cref="ActiveEquipmentDto"/>.
    /// Only manufacturing equipment has rate information; general equipment returns zero rates.
    /// </summary>
    /// <param name="equipment">The equipment entity to map.</param>
    /// <returns>An <see cref="ActiveEquipmentDto"/> populated from the entity.</returns>
    public static ActiveEquipmentDto ToActiveDto(this Equipment equipment)
    {
        var (hourlyRate, setupFee) = equipment is ManufacturingEquipment mfg
            ? (mfg.HourlyRateTHB, mfg.SetupFeeTHB)
            : (0m, 0m);

        return new ActiveEquipmentDto
        {
            Id = equipment.Id,
            AssetCode = equipment.AssetCode,
            Name = equipment.Name,
            Category = equipment.Category,
            HourlyRateTHB = hourlyRate,
            SetupFeeTHB = setupFee,
            IsOutsourced = false
        };
    }

    /// <summary>
    /// Extracts manufacturing spec properties from an equipment entity as a dictionary.
    /// Returns null for general (non-manufacturing) equipment.
    /// </summary>
    /// <param name="equipment">The equipment entity.</param>
    /// <returns>A dictionary of spec key-value pairs, or null if not applicable.</returns>
    private static Dictionary<string, object?>? ToSpecDictionary(this Equipment equipment) =>
        equipment switch
        {
            FdmPrinterEquipment fdm => new Dictionary<string, object?>
            {
                [nameof(FdmPrinterEquipment.BuildVolumeXMm)] = fdm.BuildVolumeXMm,
                [nameof(FdmPrinterEquipment.BuildVolumeYMm)] = fdm.BuildVolumeYMm,
                [nameof(FdmPrinterEquipment.BuildVolumeZMm)] = fdm.BuildVolumeZMm,
                [nameof(FdmPrinterEquipment.NozzleDiameterMm)] = fdm.NozzleDiameterMm,
                [nameof(FdmPrinterEquipment.MaxNozzleTempC)] = fdm.MaxNozzleTempC,
                [nameof(FdmPrinterEquipment.NumberOfExtruders)] = fdm.NumberOfExtruders,
                [nameof(FdmPrinterEquipment.MinLayerHeightMm)] = fdm.MinLayerHeightMm,
                [nameof(FdmPrinterEquipment.MaxLayerHeightMm)] = fdm.MaxLayerHeightMm,
                [nameof(ManufacturingEquipment.HourlyRateTHB)] = fdm.HourlyRateTHB,
                [nameof(ManufacturingEquipment.SetupFeeTHB)] = fdm.SetupFeeTHB
            },
            SlaPrinterEquipment sla => new Dictionary<string, object?>
            {
                [nameof(SlaPrinterEquipment.BuildVolumeXMm)] = sla.BuildVolumeXMm,
                [nameof(SlaPrinterEquipment.BuildVolumeYMm)] = sla.BuildVolumeYMm,
                [nameof(SlaPrinterEquipment.BuildVolumeZMm)] = sla.BuildVolumeZMm,
                [nameof(SlaPrinterEquipment.XyResolutionMm)] = sla.XyResolutionMm,
                [nameof(SlaPrinterEquipment.LightSourceType)] = sla.LightSourceType.ToString(),
                [nameof(SlaPrinterEquipment.WavelengthNm)] = sla.WavelengthNm,
                [nameof(ManufacturingEquipment.HourlyRateTHB)] = sla.HourlyRateTHB,
                [nameof(ManufacturingEquipment.SetupFeeTHB)] = sla.SetupFeeTHB
            },
            CncMachineEquipment cnc => new Dictionary<string, object?>
            {
                [nameof(CncMachineEquipment.XTravelMm)] = cnc.XTravelMm,
                [nameof(CncMachineEquipment.YTravelMm)] = cnc.YTravelMm,
                [nameof(CncMachineEquipment.ZTravelMm)] = cnc.ZTravelMm,
                [nameof(CncMachineEquipment.MaxSpindleSpeedRpm)] = cnc.MaxSpindleSpeedRpm,
                [nameof(CncMachineEquipment.MaxSpindlePowerKw)] = cnc.MaxSpindlePowerKw,
                [nameof(CncMachineEquipment.NumberOfAxes)] = cnc.NumberOfAxes,
                [nameof(CncMachineEquipment.ToolInterface)] = cnc.ToolInterface.ToString(),
                [nameof(CncMachineEquipment.MaxToolDiameterMm)] = cnc.MaxToolDiameterMm,
                [nameof(CncMachineEquipment.ControllerBrand)] = cnc.ControllerBrand,
                [nameof(ManufacturingEquipment.HourlyRateTHB)] = cnc.HourlyRateTHB,
                [nameof(ManufacturingEquipment.SetupFeeTHB)] = cnc.SetupFeeTHB
            },
            Scanner3DEquipment scanner => new Dictionary<string, object?>
            {
                [nameof(Scanner3DEquipment.MaxScanVolumeM3)] = scanner.MaxScanVolumeM3,
                [nameof(Scanner3DEquipment.AccuracyMm)] = scanner.AccuracyMm,
                [nameof(Scanner3DEquipment.ScannerTechnology)] = scanner.ScannerTechnology.ToString(),
                [nameof(ManufacturingEquipment.HourlyRateTHB)] = scanner.HourlyRateTHB,
                [nameof(ManufacturingEquipment.SetupFeeTHB)] = scanner.SetupFeeTHB
            },
            InjectionMoldingEquipment im => new Dictionary<string, object?>
            {
                [nameof(InjectionMoldingEquipment.MaxMoldXMm)] = im.MaxMoldXMm,
                [nameof(InjectionMoldingEquipment.MaxMoldYMm)] = im.MaxMoldYMm,
                [nameof(InjectionMoldingEquipment.MaxMoldZMm)] = im.MaxMoldZMm,
                [nameof(InjectionMoldingEquipment.MaxShotSizeG)] = im.MaxShotSizeG,
                [nameof(InjectionMoldingEquipment.MaxTempC)] = im.MaxTempC,
                [nameof(InjectionMoldingEquipment.MaxInjectionPressureBar)] = im.MaxInjectionPressureBar,
                [nameof(ManufacturingEquipment.HourlyRateTHB)] = im.HourlyRateTHB,
                [nameof(ManufacturingEquipment.SetupFeeTHB)] = im.SetupFeeTHB
            },
            _ => null
        };

    /// <summary>
    /// Applies spec values from a command dictionary to an <see cref="FdmPrinterEquipment"/> entity.
    /// </summary>
    /// <param name="equipment">The FDM printer entity to populate.</param>
    /// <param name="spec">The spec dictionary from the command.</param>
    public static void ApplySpec(this FdmPrinterEquipment equipment, Dictionary<string, object?> spec)
    {
        if (spec.TryGetValue(nameof(FdmPrinterEquipment.BuildVolumeXMm), out var x) && x is not null)
            equipment.BuildVolumeXMm = Convert.ToDecimal(x);
        if (spec.TryGetValue(nameof(FdmPrinterEquipment.BuildVolumeYMm), out var y) && y is not null)
            equipment.BuildVolumeYMm = Convert.ToDecimal(y);
        if (spec.TryGetValue(nameof(FdmPrinterEquipment.BuildVolumeZMm), out var z) && z is not null)
            equipment.BuildVolumeZMm = Convert.ToDecimal(z);
        if (spec.TryGetValue(nameof(FdmPrinterEquipment.NozzleDiameterMm), out var nozzle) && nozzle is not null)
            equipment.NozzleDiameterMm = Convert.ToDecimal(nozzle);
        if (spec.TryGetValue(nameof(FdmPrinterEquipment.MaxNozzleTempC), out var temp) && temp is not null)
            equipment.MaxNozzleTempC = Convert.ToDecimal(temp);
        if (spec.TryGetValue(nameof(FdmPrinterEquipment.NumberOfExtruders), out var extruders) && extruders is not null)
            equipment.NumberOfExtruders = Convert.ToInt32(extruders);
        if (spec.TryGetValue(nameof(FdmPrinterEquipment.MinLayerHeightMm), out var minLayer) && minLayer is not null)
            equipment.MinLayerHeightMm = Convert.ToDecimal(minLayer);
        if (spec.TryGetValue(nameof(FdmPrinterEquipment.MaxLayerHeightMm), out var maxLayer) && maxLayer is not null)
            equipment.MaxLayerHeightMm = Convert.ToDecimal(maxLayer);
        if (spec.TryGetValue(nameof(ManufacturingEquipment.HourlyRateTHB), out var hourly) && hourly is not null)
            equipment.HourlyRateTHB = Convert.ToDecimal(hourly);
        if (spec.TryGetValue(nameof(ManufacturingEquipment.SetupFeeTHB), out var setup) && setup is not null)
            equipment.SetupFeeTHB = Convert.ToDecimal(setup);
    }

    /// <summary>
    /// Applies spec values from a command dictionary to an <see cref="SlaPrinterEquipment"/> entity.
    /// </summary>
    /// <param name="equipment">The SLA printer entity to populate.</param>
    /// <param name="spec">The spec dictionary from the command.</param>
    public static void ApplySpec(this SlaPrinterEquipment equipment, Dictionary<string, object?> spec)
    {
        if (spec.TryGetValue(nameof(SlaPrinterEquipment.BuildVolumeXMm), out var x) && x is not null)
            equipment.BuildVolumeXMm = Convert.ToDecimal(x);
        if (spec.TryGetValue(nameof(SlaPrinterEquipment.BuildVolumeYMm), out var y) && y is not null)
            equipment.BuildVolumeYMm = Convert.ToDecimal(y);
        if (spec.TryGetValue(nameof(SlaPrinterEquipment.BuildVolumeZMm), out var z) && z is not null)
            equipment.BuildVolumeZMm = Convert.ToDecimal(z);
        if (spec.TryGetValue(nameof(SlaPrinterEquipment.XyResolutionMm), out var res) && res is not null)
            equipment.XyResolutionMm = Convert.ToDecimal(res);
        if (spec.TryGetValue(nameof(SlaPrinterEquipment.WavelengthNm), out var wave) && wave is not null)
            equipment.WavelengthNm = Convert.ToInt32(wave);
        if (spec.TryGetValue(nameof(ManufacturingEquipment.HourlyRateTHB), out var hourly) && hourly is not null)
            equipment.HourlyRateTHB = Convert.ToDecimal(hourly);
        if (spec.TryGetValue(nameof(ManufacturingEquipment.SetupFeeTHB), out var setup) && setup is not null)
            equipment.SetupFeeTHB = Convert.ToDecimal(setup);
    }

    /// <summary>
    /// Applies spec values from a command dictionary to a <see cref="CncMachineEquipment"/> entity.
    /// </summary>
    /// <param name="equipment">The CNC machine entity to populate.</param>
    /// <param name="spec">The spec dictionary from the command.</param>
    public static void ApplySpec(this CncMachineEquipment equipment, Dictionary<string, object?> spec)
    {
        if (spec.TryGetValue(nameof(CncMachineEquipment.XTravelMm), out var x) && x is not null)
            equipment.XTravelMm = Convert.ToDecimal(x);
        if (spec.TryGetValue(nameof(CncMachineEquipment.YTravelMm), out var y) && y is not null)
            equipment.YTravelMm = Convert.ToDecimal(y);
        if (spec.TryGetValue(nameof(CncMachineEquipment.ZTravelMm), out var z) && z is not null)
            equipment.ZTravelMm = Convert.ToDecimal(z);
        if (spec.TryGetValue(nameof(CncMachineEquipment.MaxSpindleSpeedRpm), out var rpm) && rpm is not null)
            equipment.MaxSpindleSpeedRpm = Convert.ToInt32(rpm);
        if (spec.TryGetValue(nameof(CncMachineEquipment.MaxSpindlePowerKw), out var power) && power is not null)
            equipment.MaxSpindlePowerKw = Convert.ToDecimal(power);
        if (spec.TryGetValue(nameof(CncMachineEquipment.NumberOfAxes), out var axes) && axes is not null)
            equipment.NumberOfAxes = Convert.ToInt32(axes);
        if (spec.TryGetValue(nameof(CncMachineEquipment.MaxToolDiameterMm), out var toolDiam) && toolDiam is not null)
            equipment.MaxToolDiameterMm = Convert.ToDecimal(toolDiam);
        if (spec.TryGetValue(nameof(CncMachineEquipment.ControllerBrand), out var ctrl))
            equipment.ControllerBrand = ctrl?.ToString();
        if (spec.TryGetValue(nameof(ManufacturingEquipment.HourlyRateTHB), out var hourly) && hourly is not null)
            equipment.HourlyRateTHB = Convert.ToDecimal(hourly);
        if (spec.TryGetValue(nameof(ManufacturingEquipment.SetupFeeTHB), out var setup) && setup is not null)
            equipment.SetupFeeTHB = Convert.ToDecimal(setup);
    }

    /// <summary>
    /// Applies spec values from a command dictionary to a <see cref="Scanner3DEquipment"/> entity.
    /// </summary>
    /// <param name="equipment">The 3D scanner entity to populate.</param>
    /// <param name="spec">The spec dictionary from the command.</param>
    public static void ApplySpec(this Scanner3DEquipment equipment, Dictionary<string, object?> spec)
    {
        if (spec.TryGetValue(nameof(Scanner3DEquipment.MaxScanVolumeM3), out var vol) && vol is not null)
            equipment.MaxScanVolumeM3 = Convert.ToDecimal(vol);
        if (spec.TryGetValue(nameof(Scanner3DEquipment.AccuracyMm), out var acc) && acc is not null)
            equipment.AccuracyMm = Convert.ToDecimal(acc);
        if (spec.TryGetValue(nameof(ManufacturingEquipment.HourlyRateTHB), out var hourly) && hourly is not null)
            equipment.HourlyRateTHB = Convert.ToDecimal(hourly);
        if (spec.TryGetValue(nameof(ManufacturingEquipment.SetupFeeTHB), out var setup) && setup is not null)
            equipment.SetupFeeTHB = Convert.ToDecimal(setup);
    }

    /// <summary>
    /// Applies spec values from a command dictionary to an <see cref="InjectionMoldingEquipment"/> entity.
    /// </summary>
    /// <param name="equipment">The injection molding entity to populate.</param>
    /// <param name="spec">The spec dictionary from the command.</param>
    public static void ApplySpec(this InjectionMoldingEquipment equipment, Dictionary<string, object?> spec)
    {
        if (spec.TryGetValue(nameof(InjectionMoldingEquipment.MaxMoldXMm), out var x) && x is not null)
            equipment.MaxMoldXMm = Convert.ToDecimal(x);
        if (spec.TryGetValue(nameof(InjectionMoldingEquipment.MaxMoldYMm), out var y) && y is not null)
            equipment.MaxMoldYMm = Convert.ToDecimal(y);
        if (spec.TryGetValue(nameof(InjectionMoldingEquipment.MaxMoldZMm), out var z) && z is not null)
            equipment.MaxMoldZMm = Convert.ToDecimal(z);
        if (spec.TryGetValue(nameof(InjectionMoldingEquipment.MaxShotSizeG), out var shot) && shot is not null)
            equipment.MaxShotSizeG = Convert.ToDecimal(shot);
        if (spec.TryGetValue(nameof(InjectionMoldingEquipment.MaxTempC), out var temp) && temp is not null)
            equipment.MaxTempC = Convert.ToInt32(temp);
        if (spec.TryGetValue(nameof(InjectionMoldingEquipment.MaxInjectionPressureBar), out var pressure) && pressure is not null)
            equipment.MaxInjectionPressureBar = Convert.ToInt32(pressure);
        if (spec.TryGetValue(nameof(ManufacturingEquipment.HourlyRateTHB), out var hourly) && hourly is not null)
            equipment.HourlyRateTHB = Convert.ToDecimal(hourly);
        if (spec.TryGetValue(nameof(ManufacturingEquipment.SetupFeeTHB), out var setup) && setup is not null)
            equipment.SetupFeeTHB = Convert.ToDecimal(setup);
    }
}
