using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Mapping;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Tests.Unit.Mapping;

public class EquipmentMappingExtensionsTests
{
    [Fact]
    public void ToSummaryDto_MapsBasicProperties()
    {
        var equipment = new FdmPrinterEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-FDM-001",
            Name = "Test Printer",
            Brand = "Prusa",
            ModelName = "MK3S+",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.Active,
            PurchasePriceTHB = 25000m,
            NextServiceDueDate = new DateOnly(2025, 6, 1),
            UpdatedAt = DateTime.UtcNow
        };

        var result = equipment.ToSummaryDto();

        Assert.Equal(equipment.Id, result.Id);
        Assert.Equal(equipment.AssetCode, result.AssetCode);
        Assert.Equal(equipment.Name, result.Name);
        Assert.Equal(equipment.Brand, result.Brand);
        Assert.Equal(equipment.ModelName, result.ModelName);
        Assert.Equal(equipment.Category, result.Category);
        Assert.Equal(equipment.Status, result.Status);
        Assert.Equal(equipment.PurchasePriceTHB, result.PurchasePriceTHB);
        Assert.Equal(equipment.NextServiceDueDate, result.NextServiceDueDate);
        Assert.Equal(equipment.UpdatedAt, result.UpdatedAt);
    }

    [Fact]
    public void ToDto_MapsFullProperties()
    {
        var equipment = new FdmPrinterEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-FDM-001",
            Name = "Test Printer",
            Brand = "Prusa",
            ModelName = "MK3S+",
            ManufacturerSerialNumber = "SN-12345",
            Category = EquipmentCategory.FdmPrinter,
            SubCategory = "Desktop",
            Status = EquipmentStatus.Active,
            PurchaseDate = new DateOnly(2024, 1, 1),
            PurchasePriceTHB = 25000m,
            WarrantyExpiryDate = new DateOnly(2026, 1, 1),
            NextServiceDueDate = new DateOnly(2025, 6, 1),
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow,
            BuildVolumeXMm = 250m,
            BuildVolumeYMm = 210m,
            BuildVolumeZMm = 200m,
            HourlyRateTHB = 500m,
            SetupFeeTHB = 100m
        };

        var result = equipment.ToDto();

        Assert.Equal(equipment.Id, result.Id);
        Assert.Equal(equipment.AssetCode, result.AssetCode);
        Assert.Equal(equipment.Name, result.Name);
        Assert.NotNull(result.Spec);
    }

    [Fact]
    public void ToActiveDto_ManufacturingEquipment_ReturnsRates()
    {
        var equipment = new CncMachineEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-CNC-001",
            Name = "Haas VF-2",
            Category = EquipmentCategory.CncMachine,
            HourlyRateTHB = 1500m,
            SetupFeeTHB = 500m
        };

        var result = equipment.ToActiveDto();

        Assert.Equal(equipment.Id, result.Id);
        Assert.Equal(1500m, result.HourlyRateTHB);
        Assert.Equal(500m, result.SetupFeeTHB);
    }

    [Fact]
    public void ToActiveDto_GeneralEquipment_ReturnsZeroRates()
    {
        var equipment = new OfficeEquipmentItem
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-OFF-001",
            Name = "Office PC",
            Category = EquipmentCategory.OfficeEquipment
        };

        var result = equipment.ToActiveDto();

        Assert.Equal(equipment.Id, result.Id);
        Assert.Equal(0m, result.HourlyRateTHB);
        Assert.Equal(0m, result.SetupFeeTHB);
    }

    [Fact]
    public void ToDto_FdmPrinter_IncludesSpec()
    {
        var equipment = new FdmPrinterEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-FDM-001",
            Name = "Test Printer",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            BuildVolumeXMm = 250m,
            BuildVolumeYMm = 210m,
            BuildVolumeZMm = 200m,
            NozzleDiameterMm = 0.4m,
            MaxNozzleTempC = 280m,
            NumberOfExtruders = 1,
            MinLayerHeightMm = 0.1m,
            MaxLayerHeightMm = 0.3m,
            HourlyRateTHB = 500m,
            SetupFeeTHB = 100m
        };

        var result = equipment.ToDto();

        Assert.NotNull(result.Spec);
        Assert.Equal(250m, result.Spec["BuildVolumeXMm"]);
        Assert.Equal(0.4m, result.Spec["NozzleDiameterMm"]);
    }

    [Fact]
    public void ToDto_SlaPrinter_IncludesSpec()
    {
        var equipment = new SlaPrinterEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-SLA-001",
            Name = "Test SLA",
            Category = EquipmentCategory.SlaPrinter,
            Status = EquipmentStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            BuildVolumeXMm = 150m,
            BuildVolumeYMm = 150m,
            BuildVolumeZMm = 300m,
            XyResolutionMm = 0.05m,
            LightSourceType = SlaLightSourceType.Laser,
            WavelengthNm = 405,
            HourlyRateTHB = 800m,
            SetupFeeTHB = 150m
        };

        var result = equipment.ToDto();

        Assert.NotNull(result.Spec);
        Assert.Equal(150m, result.Spec["BuildVolumeXMm"]);
    }

    [Fact]
    public void ToDto_CncMachine_IncludesSpec()
    {
        var equipment = new CncMachineEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-CNC-001",
            Name = "Test CNC",
            Category = EquipmentCategory.CncMachine,
            Status = EquipmentStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            XTravelMm = 500m,
            YTravelMm = 400m,
            ZTravelMm = 500m,
            MaxSpindleSpeedRpm = 10000,
            MaxSpindlePowerKw = 15m,
            NumberOfAxes = 3,
            ToolInterface = CncToolInterface.Cat,
            MaxToolDiameterMm = 100m,
            ControllerBrand = "Haas",
            HourlyRateTHB = 1500m,
            SetupFeeTHB = 500m
        };

        var result = equipment.ToDto();

        Assert.NotNull(result.Spec);
        Assert.Equal(500m, result.Spec["XTravelMm"]);
    }

    [Fact]
    public void ToDto_Scanner3D_IncludesSpec()
    {
        var equipment = new Scanner3DEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-3DS-001",
            Name = "Test Scanner",
            Category = EquipmentCategory.Scanner3D,
            Status = EquipmentStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            MaxScanVolumeM3 = 0.5m,
            AccuracyMm = 0.05m,
            ScannerTechnology = Scanner3DTechnology.Laser,
            HourlyRateTHB = 600m,
            SetupFeeTHB = 200m
        };

        var result = equipment.ToDto();

        Assert.NotNull(result.Spec);
        Assert.Equal(0.5m, result.Spec["MaxScanVolumeM3"]);
    }

    [Fact]
    public void ToDto_InjectionMolding_IncludesSpec()
    {
        var equipment = new InjectionMoldingEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-IM-001",
            Name = "Test IM",
            Category = EquipmentCategory.InjectionMolding,
            Status = EquipmentStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            MaxMoldXMm = 500m,
            MaxMoldYMm = 500m,
            MaxMoldZMm = 500m,
            MaxShotSizeG = 500m,
            MaxTempC = 400,
            MaxInjectionPressureBar = 2000,
            HourlyRateTHB = 2000m,
            SetupFeeTHB = 1000m
        };

        var result = equipment.ToDto();

        Assert.NotNull(result.Spec);
        Assert.Equal(500m, result.Spec["MaxMoldXMm"]);
    }

    [Fact]
    public void ToDto_OfficeEquipment_ReturnsNullSpec()
    {
        var equipment = new OfficeEquipmentItem
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-OFF-001",
            Name = "Office PC",
            Category = EquipmentCategory.OfficeEquipment,
            Status = EquipmentStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = equipment.ToDto();

        Assert.Null(result.Spec);
    }

    [Fact]
    public void ApplySpec_FdmPrinter_PopulatesProperties()
    {
        var equipment = new FdmPrinterEquipment();
        var spec = new Dictionary<string, object?>
        {
            ["BuildVolumeXMm"] = 250,
            ["BuildVolumeYMm"] = 210,
            ["BuildVolumeZMm"] = 200,
            ["NozzleDiameterMm"] = 0.4,
            ["MaxNozzleTempC"] = 280,
            ["NumberOfExtruders"] = 1,
            ["MinLayerHeightMm"] = 0.1,
            ["MaxLayerHeightMm"] = 0.3,
            ["HourlyRateTHB"] = 500,
            ["SetupFeeTHB"] = 100
        };

        equipment.ApplySpec(spec);

        Assert.Equal(250m, equipment.BuildVolumeXMm);
        Assert.Equal(0.4m, equipment.NozzleDiameterMm);
        Assert.Equal(500m, equipment.HourlyRateTHB);
    }
}
