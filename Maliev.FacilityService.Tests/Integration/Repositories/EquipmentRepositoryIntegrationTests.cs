using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Infrastructure.Data;
using Maliev.FacilityService.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Maliev.FacilityService.Tests.Integration.Repositories;

[Collection("PostgresCollection")]
public class EquipmentRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private FacilityDbContext _context = null!;
    private EquipmentRepository _repository = null!;

    public EquipmentRepositoryIntegrationTests(PostgresFixture fixture)
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

        _repository = new EquipmentRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    [Fact]
    public async Task AddAsync_SavesEquipment_ReturnsEquipmentWithId()
    {
        var equipment = new FdmPrinterEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-FDM-TEST-001",
            Name = "Test FDM Printer",
            Brand = "TestBrand",
            ModelName = "TestModel",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.Active,
            PurchasePriceTHB = 50000m,
            HourlyRateTHB = 200m,
            SetupFeeTHB = 50m,
            BuildVolumeXMm = 200m,
            BuildVolumeYMm = 200m,
            BuildVolumeZMm = 200m,
            NozzleDiameterMm = 0.4m,
            MaxNozzleTempC = 280m,
            NumberOfExtruders = 1,
            MinLayerHeightMm = 0.05m,
            MaxLayerHeightMm = 0.35m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _repository.AddAsync(equipment);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.NotEqual(DateTime.MinValue, result.CreatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingEquipment_ReturnsEquipment()
    {
        var equipment = new FdmPrinterEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-FDM-TEST-002",
            Name = "Test FDM Printer 2",
            Brand = "TestBrand",
            ModelName = "TestModel",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.Active,
            PurchasePriceTHB = 50000m,
            HourlyRateTHB = 200m,
            SetupFeeTHB = 50m,
            BuildVolumeXMm = 200m,
            BuildVolumeYMm = 200m,
            BuildVolumeZMm = 200m,
            NozzleDiameterMm = 0.4m,
            MaxNozzleTempC = 280m,
            NumberOfExtruders = 1,
            MinLayerHeightMm = 0.05m,
            MaxLayerHeightMm = 0.35m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Equipments.Add(equipment);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(equipment.Id);

        Assert.NotNull(result);
        Assert.Equal(equipment.Id, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingEquipment_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task ExistsByNameAsync_ExistingName_ReturnsTrue()
    {
        var equipment = new FdmPrinterEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-FDM-TEST-003",
            Name = "Unique Test Printer Name",
            Brand = "TestBrand",
            ModelName = "TestModel",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.Active,
            PurchasePriceTHB = 50000m,
            HourlyRateTHB = 200m,
            SetupFeeTHB = 50m,
            BuildVolumeXMm = 200m,
            BuildVolumeYMm = 200m,
            BuildVolumeZMm = 200m,
            NozzleDiameterMm = 0.4m,
            MaxNozzleTempC = 280m,
            NumberOfExtruders = 1,
            MinLayerHeightMm = 0.05m,
            MaxLayerHeightMm = 0.35m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Equipments.Add(equipment);
        await _context.SaveChangesAsync();

        var result = await _repository.ExistsByNameAsync("Unique Test Printer Name");

        Assert.True(result);
    }

    [Fact]
    public async Task ExistsByNameAsync_NonExistingName_ReturnsFalse()
    {
        var result = await _repository.ExistsByNameAsync("NonExisting Name");
        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_ExistingEquipment_UpdatesSuccessfully()
    {
        var equipment = new FdmPrinterEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-FDM-TEST-004",
            Name = "Original Name",
            Brand = "TestBrand",
            ModelName = "TestModel",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.Active,
            PurchasePriceTHB = 50000m,
            HourlyRateTHB = 200m,
            SetupFeeTHB = 50m,
            BuildVolumeXMm = 200m,
            BuildVolumeYMm = 200m,
            BuildVolumeZMm = 200m,
            NozzleDiameterMm = 0.4m,
            MaxNozzleTempC = 280m,
            NumberOfExtruders = 1,
            MinLayerHeightMm = 0.05m,
            MaxLayerHeightMm = 0.35m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Equipments.Add(equipment);
        await _context.SaveChangesAsync();

        equipment.Name = "Updated Name";
        equipment.Status = EquipmentStatus.UnderMaintenance;

        var result = await _repository.UpdateAsync(equipment);

        Assert.Equal("Updated Name", result.Name);
        Assert.Equal(EquipmentStatus.UnderMaintenance, result.Status);
    }

    [Fact]
    public async Task DeleteAsync_ExistingEquipment_ReturnsTrue()
    {
        var equipment = new FdmPrinterEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-FDM-TEST-005",
            Name = "To Be Deleted",
            Brand = "TestBrand",
            ModelName = "TestModel",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.Active,
            PurchasePriceTHB = 50000m,
            HourlyRateTHB = 200m,
            SetupFeeTHB = 50m,
            BuildVolumeXMm = 200m,
            BuildVolumeYMm = 200m,
            BuildVolumeZMm = 200m,
            NozzleDiameterMm = 0.4m,
            MaxNozzleTempC = 280m,
            NumberOfExtruders = 1,
            MinLayerHeightMm = 0.05m,
            MaxLayerHeightMm = 0.35m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Equipments.Add(equipment);
        await _context.SaveChangesAsync();

        var result = await _repository.DeleteAsync(equipment.Id);

        Assert.True(result);

        var deleted = await _context.Equipments.FindAsync(equipment.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingEquipment_ReturnsFalse()
    {
        var result = await _repository.DeleteAsync(Guid.NewGuid());
        Assert.False(result);
    }

    [Fact]
    public async Task GetAllAsync_NoFilters_ReturnsAllEquipment()
    {
        await SeedMultipleEquipmentAsync();

        var (items, totalCount) = await _repository.GetAllAsync();

        Assert.Equal(5, totalCount);
        Assert.Equal(5, items.Count);
    }

    [Fact]
    public async Task GetAllAsync_FilterByCategory_ReturnsFilteredResults()
    {
        await SeedMultipleEquipmentAsync();

        var (items, totalCount) = await _repository.GetAllAsync(
            filters: new Application.Interfaces.EquipmentFilter { Category = EquipmentCategory.FdmPrinter });

        Assert.Equal(2, totalCount);
        Assert.All(items, item => Assert.Equal(EquipmentCategory.FdmPrinter, item.Category));
    }

    [Fact]
    public async Task GetAllAsync_FilterByStatus_ReturnsFilteredResults()
    {
        await SeedMultipleEquipmentAsync();

        var (items, totalCount) = await _repository.GetAllAsync(
            filters: new Application.Interfaces.EquipmentFilter { Status = EquipmentStatus.Active });

        Assert.Equal(3, totalCount);
        Assert.All(items, item => Assert.Equal(EquipmentStatus.Active, item.Status));
    }

    [Fact]
    public async Task GetAllAsync_FilterByNameContains_ReturnsFilteredResults()
    {
        await SeedMultipleEquipmentAsync();

        var (items, totalCount) = await _repository.GetAllAsync(
            filters: new Application.Interfaces.EquipmentFilter { NameContains = "Ultimaker" });

        Assert.Single(items);
        Assert.Contains("Ultimaker", items[0].Name);
    }

    [Fact]
    public async Task GetAllAsync_Pagination_ReturnsCorrectPage()
    {
        await SeedMultipleEquipmentAsync();

        var (items, totalCount) = await _repository.GetAllAsync(
            pagination: new Application.Interfaces.Pagination { Page = 2, PageSize = 2 });

        Assert.Equal(5, totalCount);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GetActiveByCategoryAsync_ReturnsOnlyActiveEquipment()
    {
        await SeedMultipleEquipmentAsync();

        var result = await _repository.GetActiveByCategoryAsync(EquipmentCategory.FdmPrinter);

        Assert.Single(result);
        Assert.Equal(EquipmentStatus.Active, result[0].Status);
    }

    [Fact]
    public async Task GetActiveByMultipleCategoriesAsync_ReturnsActiveEquipmentForAllCategories()
    {
        await SeedMultipleEquipmentAsync();

        var categories = new HashSet<EquipmentCategory>
        {
            EquipmentCategory.FdmPrinter,
            EquipmentCategory.SlaPrinter
        };

        var result = await _repository.GetActiveByMultipleCategoriesAsync(categories);

        Assert.Equal(2, result.Count);
        Assert.All(result, item => Assert.Equal(EquipmentStatus.Active, item.Status));
    }

    private async Task SeedMultipleEquipmentAsync()
    {
        var fdm1 = new FdmPrinterEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-FDM-001",
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
            AssetCode = "MAL-FDM-002",
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
            AssetCode = "MAL-SLA-001",
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
            AssetCode = "MAL-CNC-001",
            Name = "HAAS VF2",
            Brand = "HAAS",
            ModelName = "VF2",
            Category = EquipmentCategory.CncMachine,
            Status = EquipmentStatus.Active,
            PurchasePriceTHB = 800000m,
            HourlyRateTHB = 2000m,
            SetupFeeTHB = 500m,
            XTravelMm = 660m,
            YTravelMm = 508m,
            ZTravelMm = 635m,
            MaxSpindleSpeedRpm = 30000,
            MaxSpindlePowerKw = 22.0m,
            NumberOfAxes = 3,
            ToolInterface = CncToolInterface.Cat,
            MaxToolDiameterMm = 76.2m,
            ControllerBrand = "HAAS",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var office1 = new OfficeEquipmentItem
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-OFF-001",
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
