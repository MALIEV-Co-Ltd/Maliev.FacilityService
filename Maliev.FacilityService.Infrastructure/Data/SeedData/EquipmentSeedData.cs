using System.Security.Cryptography;
using System.Text;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Infrastructure.Data.SeedData;

/// <summary>
/// Provides static seed data for manufacturing equipment in the facility.
/// Uses deterministic GUIDs based on SHA256 to ensure consistent IDs across environments.
/// </summary>
public static class EquipmentSeedData
{
    private static Guid G(string code)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        var bytes = new byte[16];
        Array.Copy(hash, bytes, 16);
        bytes[6] = (byte)((bytes[6] & 0x0f) | 0x50);
        bytes[8] = (byte)((bytes[8] & 0x3f) | 0x80);
        return new Guid(bytes);
    }

    private static DateTime UtcNow => DateTime.UtcNow;

    public static IEnumerable<FdmPrinterEquipment> GetFdmPrinters() =>
    [
        new()
        {
            Id = G("FAC_FDM_001"),
            AssetCode = "MAL-FDM-001",
            Brand = "Prusa",
            ModelName = "i3 MK3S+",
            Name = "FDM Printer 1",
            ManufacturerSerialNumber = "FDM-MAL-001-A",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.Active,
            PurchaseDate = new DateOnly(2024, 1, 15),
            PurchasePriceTHB = 45000m,
            WarrantyExpiryDate = new DateOnly(2027, 1, 15),
            NextServiceDueDate = new DateOnly(2026, 7, 15),
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
            HourlyRateTHB = 150m,
            SetupFeeTHB = 100m,
            ExtendedProperties = null,
            BuildVolumeXMm = 250m,
            BuildVolumeYMm = 210m,
            BuildVolumeZMm = 210m,
            NozzleDiameterMm = 0.4m,
            MaxNozzleTempC = 300m,
            NumberOfExtruders = 1,
            MinLayerHeightMm = 0.05m,
            MaxLayerHeightMm = 0.35m
        },
        new()
        {
            Id = G("FAC_FDM_002"),
            AssetCode = "MAL-FDM-002",
            Brand = "Prusa",
            ModelName = "i3 MK3S+",
            Name = "FDM Printer 2",
            ManufacturerSerialNumber = "FDM-MAL-002-B",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.Active,
            PurchaseDate = new DateOnly(2024, 6, 1),
            PurchasePriceTHB = 45000m,
            WarrantyExpiryDate = new DateOnly(2027, 6, 1),
            NextServiceDueDate = new DateOnly(2026, 12, 1),
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
            HourlyRateTHB = 150m,
            SetupFeeTHB = 100m,
            ExtendedProperties = null,
            BuildVolumeXMm = 250m,
            BuildVolumeYMm = 210m,
            BuildVolumeZMm = 210m,
            NozzleDiameterMm = 0.4m,
            MaxNozzleTempC = 300m,
            NumberOfExtruders = 1,
            MinLayerHeightMm = 0.05m,
            MaxLayerHeightMm = 0.35m
        }
    ];

    public static IEnumerable<SlaPrinterEquipment> GetSlaPrinters() =>
    [
        new()
        {
            Id = G("FAC_SLA_001"),
            AssetCode = "MAL-SLA-001",
            Brand = "Anycubic",
            ModelName = "Photon Mono X",
            Name = "SLA Printer 1",
            ManufacturerSerialNumber = "SLA-MAL-001-A",
            Category = EquipmentCategory.SlaPrinter,
            Status = EquipmentStatus.Active,
            PurchaseDate = new DateOnly(2024, 3, 20),
            PurchasePriceTHB = 12000m,
            WarrantyExpiryDate = new DateOnly(2027, 3, 20),
            NextServiceDueDate = new DateOnly(2026, 9, 20),
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
            HourlyRateTHB = 200m,
            SetupFeeTHB = 150m,
            ExtendedProperties = null,
            BuildVolumeXMm = 98m,
            BuildVolumeYMm = 54m,
            BuildVolumeZMm = 148m,
            XyResolutionMm = 0.05m,
            LightSourceType = SlaLightSourceType.Led,
            WavelengthNm = 405
        },
        new()
        {
            Id = G("FAC_SLA_002"),
            AssetCode = "MAL-SLA-002",
            Brand = "Anycubic",
            ModelName = "Photon Mono X",
            Name = "SLA Printer 2",
            ManufacturerSerialNumber = "SLA-MAL-002-B",
            Category = EquipmentCategory.SlaPrinter,
            Status = EquipmentStatus.Active,
            PurchaseDate = new DateOnly(2024, 8, 10),
            PurchasePriceTHB = 12000m,
            WarrantyExpiryDate = new DateOnly(2027, 8, 10),
            NextServiceDueDate = new DateOnly(2027, 2, 10),
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
            HourlyRateTHB = 200m,
            SetupFeeTHB = 150m,
            ExtendedProperties = null,
            BuildVolumeXMm = 98m,
            BuildVolumeYMm = 54m,
            BuildVolumeZMm = 148m,
            XyResolutionMm = 0.05m,
            LightSourceType = SlaLightSourceType.Led,
            WavelengthNm = 405
        }
    ];

    public static IEnumerable<CncMachineEquipment> GetCncMachines() =>
    [
        new()
        {
            Id = G("FAC_CNC_001"),
            AssetCode = "MAL-CNC-001",
            Brand = "HAAS",
            ModelName = "VF2",
            Name = "HAAS VF2 CNC",
            ManufacturerSerialNumber = "CNC-MAL-001-A",
            Category = EquipmentCategory.CncMachine,
            Status = EquipmentStatus.Active,
            PurchaseDate = new DateOnly(2023, 11, 5),
            PurchasePriceTHB = 2500000m,
            WarrantyExpiryDate = new DateOnly(2026, 11, 5),
            NextServiceDueDate = new DateOnly(2026, 5, 5),
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
            HourlyRateTHB = 2500m,
            SetupFeeTHB = 2000m,
            ExtendedProperties = null,
            XTravelMm = 840m,
            YTravelMm = 560m,
            ZTravelMm = 510m,
            MaxSpindleSpeedRpm = 15000,
            MaxSpindlePowerKw = 22m,
            NumberOfAxes = 3,
            ToolInterface = CncToolInterface.Iso,
            MaxToolDiameterMm = 76m,
            ControllerBrand = "Haas"
        }
    ];

    public static IEnumerable<Scanner3DEquipment> GetScanners() =>
    [
        new()
        {
            Id = G("FAC_SCN_001"),
            AssetCode = "MAL-SCN-001",
            Brand = "Creality",
            ModelName = "Raptor X",
            Name = "3D Scanner 1",
            ManufacturerSerialNumber = "SCN-MAL-001-A",
            Category = EquipmentCategory.Scanner3D,
            Status = EquipmentStatus.Active,
            PurchaseDate = new DateOnly(2024, 5, 15),
            PurchasePriceTHB = 28000m,
            WarrantyExpiryDate = new DateOnly(2027, 5, 15),
            NextServiceDueDate = new DateOnly(2026, 11, 15),
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
            HourlyRateTHB = 500m,
            SetupFeeTHB = 300m,
            ExtendedProperties = null,
            MaxScanVolumeM3 = 0.5m,
            AccuracyMm = 0.05m,
            ScanResolutions = "[\"0.05mm\", \"0.1mm\", \"0.2mm\"]",
            ScannerTechnology = Scanner3DTechnology.StructuredLight
        }
    ];

    public static IEnumerable<Equipment> GetAll()
    {
        foreach (var item in GetFdmPrinters()) yield return item;
        foreach (var item in GetSlaPrinters()) yield return item;
        foreach (var item in GetCncMachines()) yield return item;
        foreach (var item in GetScanners()) yield return item;
    }
}
