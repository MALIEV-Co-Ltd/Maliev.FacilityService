using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Queries.ListEquipments;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Infrastructure.Data;
using Maliev.FacilityService.Infrastructure.Data.Repositories;
using Maliev.FacilityService.Tests;
using Microsoft.EntityFrameworkCore;

namespace Maliev.FacilityService.Tests.Integration.Queries;

[Collection("PostgresCollection")]
public class ListEquipmentsIntegrationTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private FacilityDbContext _context = null!;
    private ListEquipmentsQueryHandler _handler = null!;

    public ListEquipmentsIntegrationTests(PostgresFixture fixture)
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

        _handler = new ListEquipmentsQueryHandler(new EquipmentRepository(_context));
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task HandleAsync_ListAll_ReturnsPagedResults()
    {
        await ClearAndSeedDataAsync();

        var query = new ListEquipmentsQuery(null, null, null, 1, 10);
        var result = await _handler.HandleAsync(query);

        Assert.NotNull(result);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(5, result.Items.Count);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);
    }

    [Fact]
    public async Task HandleAsync_FilterByCategory_ReturnsOnlyMatchingCategory()
    {
        await ClearAndSeedDataAsync();

        var query = new ListEquipmentsQuery(EquipmentCategory.FdmPrinter, null, null, 1, 10);
        var result = await _handler.HandleAsync(query);

        Assert.NotNull(result);
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, item => Assert.Equal(EquipmentCategory.FdmPrinter, item.Category));
    }

    [Fact]
    public async Task HandleAsync_FilterByStatus_ReturnsOnlyMatchingStatus()
    {
        await ClearAndSeedDataAsync();

        var query = new ListEquipmentsQuery(null, EquipmentStatus.Active, null, 1, 10);
        var result = await _handler.HandleAsync(query);

        Assert.NotNull(result);
        Assert.Equal(3, result.TotalCount);
        Assert.All(result.Items, item => Assert.Equal(EquipmentStatus.Active, item.Status));
    }

    [Fact]
    public async Task HandleAsync_FilterByCategoryAndStatus_ReturnsIntersection()
    {
        await ClearAndSeedDataAsync();

        var query = new ListEquipmentsQuery(EquipmentCategory.FdmPrinter, EquipmentStatus.Active, null, 1, 10);
        var result = await _handler.HandleAsync(query);

        Assert.NotNull(result);
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(EquipmentCategory.FdmPrinter, result.Items[0].Category);
        Assert.Equal(EquipmentStatus.Active, result.Items[0].Status);
    }

    [Fact]
    public async Task HandleAsync_Pagination_ReturnsCorrectPage()
    {
        await ClearAndSeedDataAsync();

        var query = new ListEquipmentsQuery(null, null, null, 2, 2);
        var result = await _handler.HandleAsync(query);

        Assert.NotNull(result);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
    }

    [Fact]
    public async Task HandleAsync_PageSizeExceedsMax_ClampedTo100()
    {
        await ClearAndSeedDataAsync();

        var query = new ListEquipmentsQuery(null, null, null, 1, 200);
        var result = await _handler.HandleAsync(query);

        Assert.NotNull(result);
        Assert.Equal(100, result.PageSize);
    }

    [Fact]
    public async Task HandleAsync_PageLessThan1_DefaultsTo1()
    {
        await ClearAndSeedDataAsync();

        var query = new ListEquipmentsQuery(null, null, null, 0, 10);
        var result = await _handler.HandleAsync(query);

        Assert.NotNull(result);
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public async Task HandleAsync_SearchByName_ReturnsMatchingItems()
    {
        await ClearAndSeedDataAsync();

        var query = new ListEquipmentsQuery(null, null, "Ultimaker", 1, 10);
        var result = await _handler.HandleAsync(query);

        Assert.NotNull(result);
        Assert.Single(result.Items);
        Assert.Contains("Ultimaker", result.Items[0].Name);
    }

    private async Task ClearAndSeedDataAsync()
    {
        _context.Equipments.RemoveRange(_context.Equipments);
        await _context.SaveChangesAsync();
        await SeedTestDataAsync();
    }

    private async Task SeedTestDataAsync()
    {
        var fdm1 = new FdmPrinterEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-FDM-0001",
            Name = "Ultimaker S5",
            Brand = "Ultimaker",
            ModelName = "S5",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.Active,
            PurchasePriceTHB = 150000m,
            HourlyRateTHB = 500m,
            SetupFeeTHB = 100m,
            BuildVolumeXMm = 330m,
            BuildVolumeYMm = 240m,
            BuildVolumeZMm = 300m,
            NozzleDiameterMm = 0.4m,
            MaxNozzleTempC = 280m,
            NumberOfExtruders = 2,
            MinLayerHeightMm = 0.02m,
            MaxLayerHeightMm = 0.6m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var fdm2 = new FdmPrinterEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-FDM-0002",
            Name = "Prusa MK4",
            Brand = "Prusa",
            ModelName = "MK4",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.UnderMaintenance,
            PurchasePriceTHB = 80000m,
            HourlyRateTHB = 300m,
            SetupFeeTHB = 50m,
            BuildVolumeXMm = 250m,
            BuildVolumeYMm = 210m,
            BuildVolumeZMm = 220m,
            NozzleDiameterMm = 0.4m,
            MaxNozzleTempC = 300m,
            NumberOfExtruders = 1,
            MinLayerHeightMm = 0.05m,
            MaxLayerHeightMm = 0.35m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var sla1 = new SlaPrinterEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-SLA-0001",
            Name = "Form 3L",
            Brand = "Formlabs",
            ModelName = "Form 3L",
            Category = EquipmentCategory.SlaPrinter,
            Status = EquipmentStatus.Active,
            PurchasePriceTHB = 200000m,
            HourlyRateTHB = 800m,
            SetupFeeTHB = 200m,
            BuildVolumeXMm = 345m,
            BuildVolumeYMm = 340m,
            BuildVolumeZMm = 300m,
            XyResolutionMm = 0.025m,
            LightSourceType = SlaLightSourceType.Laser,
            WavelengthNm = 405,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var cnc1 = new CncMachineEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-CNC-0001",
            Name = "Shapeoko XXL",
            Brand = "Carbide3D",
            ModelName = "XXL",
            Category = EquipmentCategory.CncMachine,
            Status = EquipmentStatus.Active,
            PurchasePriceTHB = 120000m,
            HourlyRateTHB = 600m,
            SetupFeeTHB = 150m,
            XTravelMm = 850m,
            YTravelMm = 850m,
            ZTravelMm = 125m,
            MaxSpindleSpeedRpm = 25000,
            MaxSpindlePowerKw = 1.5m,
            NumberOfAxes = 3,
            ToolInterface = CncToolInterface.Iso,
            MaxToolDiameterMm = 6.35m,
            ControllerBrand = "Carbide Motion",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var office1 = new OfficeEquipmentItem
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-OFF-0001",
            Name = "Office Printer",
            Brand = "HP",
            ModelName = "LaserJet Pro",
            Category = EquipmentCategory.OfficeEquipment,
            Status = EquipmentStatus.OnLoan,
            PurchasePriceTHB = 15000m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Equipments.AddRange(fdm1, fdm2, sla1, cnc1, office1);
        await _context.SaveChangesAsync();
    }
}
