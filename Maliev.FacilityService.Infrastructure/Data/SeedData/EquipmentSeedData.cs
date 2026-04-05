using System.Security.Cryptography;
using System.Text;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Infrastructure.Data.SeedData;

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
            Brand = "Bambulab",
            ModelName = "X1C",
            Name = "Bambulab X1C #1",
            ManufacturerSerialNumber = "FDM-X1C-001-A",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.Active,
            PurchaseDate = new DateOnly(2024, 1, 15),
            PurchasePriceTHB = 35000m,
            WarrantyExpiryDate = new DateOnly(2027, 1, 15),
            NextServiceDueDate = new DateOnly(2026, 7, 15),
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
            HourlyRateTHB = 200m,
            SetupFeeTHB = 150m,
            ExtendedProperties = null,
            BuildVolumeXMm = 256m,
            BuildVolumeYMm = 256m,
            BuildVolumeZMm = 256m,
            NozzleDiameterMm = 0.4m,
            MaxNozzleTempC = 300m,
            NumberOfExtruders = 2,
            MinLayerHeightMm = 0.05m,
            MaxLayerHeightMm = 0.35m
        },
        new()
        {
            Id = G("FAC_FDM_002"),
            AssetCode = "MAL-FDM-002",
            Brand = "Bambulab",
            ModelName = "X1C",
            Name = "Bambulab X1C #2",
            ManufacturerSerialNumber = "FDM-X1C-002-B",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.Active,
            PurchaseDate = new DateOnly(2024, 3, 1),
            PurchasePriceTHB = 35000m,
            WarrantyExpiryDate = new DateOnly(2027, 3, 1),
            NextServiceDueDate = new DateOnly(2026, 9, 1),
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
            HourlyRateTHB = 200m,
            SetupFeeTHB = 150m,
            ExtendedProperties = null,
            BuildVolumeXMm = 256m,
            BuildVolumeYMm = 256m,
            BuildVolumeZMm = 256m,
            NozzleDiameterMm = 0.4m,
            MaxNozzleTempC = 300m,
            NumberOfExtruders = 2,
            MinLayerHeightMm = 0.05m,
            MaxLayerHeightMm = 0.35m
        },
        new()
        {
            Id = G("FAC_FDM_003"),
            AssetCode = "MAL-FDM-003",
            Brand = "Prusa",
            ModelName = "i3 MK3S+",
            Name = "Prusa MK3 #1",
            ManufacturerSerialNumber = "FDM-MK3-003-A",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.UnderMaintenance,
            PurchaseDate = new DateOnly(2023, 6, 1),
            PurchasePriceTHB = 45000m,
            WarrantyExpiryDate = new DateOnly(2026, 6, 1),
            NextServiceDueDate = new DateOnly(2026, 4, 1),
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
            Id = G("FAC_FDM_004"),
            AssetCode = "MAL-FDM-004",
            Brand = "Prusa",
            ModelName = "i3 MK3S+",
            Name = "Prusa MK3 #2",
            ManufacturerSerialNumber = "FDM-MK3-004-B",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.UnderMaintenance,
            PurchaseDate = new DateOnly(2023, 8, 15),
            PurchasePriceTHB = 45000m,
            WarrantyExpiryDate = new DateOnly(2026, 8, 15),
            NextServiceDueDate = new DateOnly(2026, 6, 15),
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
            Brand = "Phrozen",
            ModelName = "Mighty 4K",
            Name = "Phrozen Mighty 4K",
            ManufacturerSerialNumber = "SLA-MIGHTY-001-A",
            Category = EquipmentCategory.SlaPrinter,
            Status = EquipmentStatus.Active,
            PurchaseDate = new DateOnly(2024, 2, 20),
            PurchasePriceTHB = 18000m,
            WarrantyExpiryDate = new DateOnly(2027, 2, 20),
            NextServiceDueDate = new DateOnly(2026, 8, 20),
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
            HourlyRateTHB = 250m,
            SetupFeeTHB = 200m,
            ExtendedProperties = null,
            BuildVolumeXMm = 120m,
            BuildVolumeYMm = 120m,
            BuildVolumeZMm = 180m,
            XyResolutionMm = 0.035m,
            LightSourceType = SlaLightSourceType.Led,
            WavelengthNm = 405
        },
        new()
        {
            Id = G("FAC_SLA_002"),
            AssetCode = "MAL-SLA-002",
            Brand = "Phrozen",
            ModelName = "Mega 8K",
            Name = "Phrozen Mega 8K",
            ManufacturerSerialNumber = "SLA-MEGA-002-A",
            Category = EquipmentCategory.SlaPrinter,
            Status = EquipmentStatus.UnderMaintenance,
            PurchaseDate = new DateOnly(2024, 5, 10),
            PurchasePriceTHB = 25000m,
            WarrantyExpiryDate = new DateOnly(2027, 5, 10),
            NextServiceDueDate = new DateOnly(2026, 3, 10),
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
            HourlyRateTHB = 300m,
            SetupFeeTHB = 250m,
            ExtendedProperties = null,
            BuildVolumeXMm = 218m,
            BuildVolumeYMm = 123m,
            BuildVolumeZMm = 235m,
            XyResolutionMm = 0.028m,
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
            Name = "HAAS VF2",
            ManufacturerSerialNumber = "CNC-VF2-001-A",
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
            Name = "Creality Raptor X",
            ManufacturerSerialNumber = "SCN-RAPTOR-001-A",
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
        },
        new()
        {
            Id = G("FAC_SCN_002"),
            AssetCode = "MAL-SCN-002",
            Brand = "Shining",
            ModelName = "EinScan Pro 2X Plus",
            Name = "Shining EinScan Pro 2X Plus",
            ManufacturerSerialNumber = "SCN-EINSCAN-002-A",
            Category = EquipmentCategory.Scanner3D,
            Status = EquipmentStatus.Active,
            PurchaseDate = new DateOnly(2024, 7, 1),
            PurchasePriceTHB = 65000m,
            WarrantyExpiryDate = new DateOnly(2027, 7, 1),
            NextServiceDueDate = new DateOnly(2026, 12, 1),
            CreatedAt = UtcNow,
            UpdatedAt = UtcNow,
            HourlyRateTHB = 800m,
            SetupFeeTHB = 500m,
            ExtendedProperties = null,
            MaxScanVolumeM3 = 0.2m,
            AccuracyMm = 0.02m,
            ScanResolutions = "[\"0.05mm\", \"0.1mm\", \"0.2mm\", \"0.3mm\"]",
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