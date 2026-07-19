using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Queries.GetActiveEquipmentsByCategory;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Infrastructure.Data;
using Maliev.FacilityService.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Maliev.FacilityService.Tests.Integration.Queries;

[Collection("PostgresCollection")]
public class GetActiveEquipmentsByCategoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private FacilityDbContext _context = null!;
    private GetActiveEquipmentsByCategoryQueryHandler _handler = null!;

    public GetActiveEquipmentsByCategoryIntegrationTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<FacilityDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;

        _context = new FacilityDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        _handler = new GetActiveEquipmentsByCategoryQueryHandler(new EquipmentRepository(_context));
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task HandleAsync_WithSpecificCategory_ReturnsOnlyActiveFdmPrinterEquipment()
    {
        var fdmPrinter = CreateFdmPrinter("FDM Printer 1", EquipmentStatus.Active);
        var slaPrinter = CreateSlaPrinter("SLA Printer 1", EquipmentStatus.Active);
        var inactiveFdmPrinter = CreateFdmPrinter("FDM Printer 2", EquipmentStatus.UnderMaintenance);

        _context.Equipments.AddRange(fdmPrinter, slaPrinter, inactiveFdmPrinter);
        await _context.SaveChangesAsync();

        var query = new GetActiveEquipmentsByCategoryQuery(EquipmentCategory.FdmPrinter);
        var result = await _handler.HandleAsync(query);

        Assert.Single(result);
        Assert.Equal(EquipmentCategory.FdmPrinter, result[0].Category);
        Assert.Equal("FDM Printer 1", result[0].Name);
        Assert.Equal(EquipmentStatus.Active, fdmPrinter.Status);
    }

    [Fact]
    public async Task HandleAsync_WithNullCategory_ReturnsAllManufacturingCategories()
    {
        var fdmPrinter = CreateFdmPrinter("FDM Printer 1", EquipmentStatus.Active);
        var slaPrinter = CreateSlaPrinter("SLA Printer 1", EquipmentStatus.Active);
        var cncMachine = CreateCncMachine("CNC Machine 1", EquipmentStatus.Active);
        var scanner3D = CreateScanner3D("Scanner 3D 1", EquipmentStatus.Active);
        var injectionMolding = CreateInjectionMolding("Injection Molding 1", EquipmentStatus.Active);

        _context.Equipments.AddRange(fdmPrinter, slaPrinter, cncMachine, scanner3D, injectionMolding);
        await _context.SaveChangesAsync();

        var query = new GetActiveEquipmentsByCategoryQuery(null);
        var result = await _handler.HandleAsync(query);

        Assert.Equal(5, result.Count);
        Assert.Contains(result, r => r.Category == EquipmentCategory.FdmPrinter);
        Assert.Contains(result, r => r.Category == EquipmentCategory.SlaPrinter);
        Assert.Contains(result, r => r.Category == EquipmentCategory.CncMachine);
        Assert.Contains(result, r => r.Category == EquipmentCategory.Scanner3D);
        Assert.Contains(result, r => r.Category == EquipmentCategory.InjectionMolding);
    }

    [Fact]
    public async Task HandleAsync_ExcludesNonManufacturingEquipment()
    {
        var fdmPrinter = CreateFdmPrinter("FDM Printer 1", EquipmentStatus.Active);
        var officeEquipment = CreateOfficeEquipment("Office PC 1", EquipmentStatus.Active);
        var itEquipment = CreateItEquipment("Dell Server 1", EquipmentStatus.Active);

        _context.Equipments.AddRange(fdmPrinter, officeEquipment, itEquipment);
        await _context.SaveChangesAsync();

        var query = new GetActiveEquipmentsByCategoryQuery(null);
        var result = await _handler.HandleAsync(query);

        Assert.Single(result);
        Assert.Equal(EquipmentCategory.FdmPrinter, result[0].Category);
    }

    private static FdmPrinterEquipment CreateFdmPrinter(string name, EquipmentStatus status)
    {
        return new FdmPrinterEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = $"MAL-FDM-{Random.Shared.Next(1000, 9999)}",
            Name = name,
            Category = EquipmentCategory.FdmPrinter,
            Status = status,
            HourlyRateTHB = 500m,
            SetupFeeTHB = 100m,
            BuildVolumeXMm = 200m,
            BuildVolumeYMm = 200m,
            BuildVolumeZMm = 200m,
            NozzleDiameterMm = 0.4m,
            MaxNozzleTempC = 250m,
            NumberOfExtruders = 1,
            MinLayerHeightMm = 0.1m,
            MaxLayerHeightMm = 0.3m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static SlaPrinterEquipment CreateSlaPrinter(string name, EquipmentStatus status)
    {
        return new SlaPrinterEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = $"MAL-SLA-{Random.Shared.Next(1000, 9999)}",
            Name = name,
            Category = EquipmentCategory.SlaPrinter,
            Status = status,
            HourlyRateTHB = 800m,
            SetupFeeTHB = 150m,
            BuildVolumeXMm = 145m,
            BuildVolumeYMm = 145m,
            BuildVolumeZMm = 175m,
            XyResolutionMm = 0.025m,
            LightSourceType = SlaLightSourceType.Laser,
            WavelengthNm = 405,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static CncMachineEquipment CreateCncMachine(string name, EquipmentStatus status)
    {
        return new CncMachineEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = $"MAL-CNC-{Random.Shared.Next(1000, 9999)}",
            Name = name,
            Category = EquipmentCategory.CncMachine,
            Status = status,
            HourlyRateTHB = 1200m,
            SetupFeeTHB = 200m,
            XTravelMm = 1000m,
            YTravelMm = 600m,
            ZTravelMm = 500m,
            MaxSpindleSpeedRpm = 18000,
            MaxSpindlePowerKw = 15m,
            NumberOfAxes = 3,
            ToolInterface = CncToolInterface.Iso,
            ControllerBrand = "Siemens",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static Scanner3DEquipment CreateScanner3D(string name, EquipmentStatus status)
    {
        return new Scanner3DEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = $"MAL-3DS-{Random.Shared.Next(1000, 9999)}",
            Name = name,
            Category = EquipmentCategory.Scanner3D,
            Status = status,
            HourlyRateTHB = 600m,
            SetupFeeTHB = 80m,
            MaxScanVolumeM3 = 0.5m,
            AccuracyMm = 0.05m,
            ScannerTechnology = Scanner3DTechnology.StructuredLight,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static InjectionMoldingEquipment CreateInjectionMolding(string name, EquipmentStatus status)
    {
        return new InjectionMoldingEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = $"MAL-INJ-{Random.Shared.Next(1000, 9999)}",
            Name = name,
            Category = EquipmentCategory.InjectionMolding,
            Status = status,
            HourlyRateTHB = 1500m,
            SetupFeeTHB = 300m,
            MaxMoldXMm = 500m,
            MaxMoldYMm = 500m,
            MaxMoldZMm = 400m,
            MaxShotSizeG = 500m,
            MaxTempC = 400,
            MaxInjectionPressureBar = 1800,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static OfficeEquipmentItem CreateOfficeEquipment(string name, EquipmentStatus status)
    {
        return new OfficeEquipmentItem
        {
            Id = Guid.NewGuid(),
            AssetCode = $"MAL-OFF-{Random.Shared.Next(1000, 9999)}",
            Name = name,
            Category = EquipmentCategory.OfficeEquipment,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    private static ITEquipmentItem CreateItEquipment(string name, EquipmentStatus status)
    {
        return new ITEquipmentItem
        {
            Id = Guid.NewGuid(),
            AssetCode = $"MAL-IT-{Random.Shared.Next(1000, 9999)}",
            Name = name,
            Category = EquipmentCategory.ITEquipment,
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
