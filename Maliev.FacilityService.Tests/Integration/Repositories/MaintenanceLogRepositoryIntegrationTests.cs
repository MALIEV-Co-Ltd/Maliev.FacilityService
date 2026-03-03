using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Infrastructure.Data;
using Maliev.FacilityService.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Maliev.FacilityService.Tests.Integration.Repositories;

[Collection("PostgresCollection")]
public class MaintenanceLogRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private FacilityDbContext _context = null!;
    private MaintenanceLogRepository _repository = null!;

    public MaintenanceLogRepositoryIntegrationTests(PostgresFixture fixture)
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

        _repository = new MaintenanceLogRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    private async Task<Equipment> SeedEquipmentAsync()
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

        _context.Equipments.Add(equipment);
        await _context.SaveChangesAsync();
        return equipment;
    }

    [Fact]
    public async Task AddAsync_SavesMaintenanceLog_ReturnsLogWithId()
    {
        var equipment = await SeedEquipmentAsync();

        var log = new EquipmentMaintenanceLog
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            LoggedByEmployeeId = Guid.NewGuid(),
            Type = MaintenanceType.Preventive,
            OccurredAt = new DateTime(2026, 3, 1, 10, 0, 0, DateTimeKind.Utc),
            Description = "Regular maintenance check",
            VendorName = "Service Co",
            CostTHB = 500m,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _repository.AddAsync(log);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(equipment.Id, result.EquipmentId);
    }

    [Fact]
    public async Task GetByEquipmentIdAsync_ReturnsLogsOrderedByDateDesc()
    {
        var equipment = await SeedEquipmentAsync();
        var employeeId = Guid.NewGuid();

        var log1 = new EquipmentMaintenanceLog
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            LoggedByEmployeeId = employeeId,
            Type = MaintenanceType.Preventive,
            OccurredAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            Description = "First maintenance",
            VendorName = "Service A",
            CostTHB = 500m,
            CreatedAt = DateTime.UtcNow
        };

        var log2 = new EquipmentMaintenanceLog
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            LoggedByEmployeeId = employeeId,
            Type = MaintenanceType.Corrective,
            OccurredAt = new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc),
            Description = "Second maintenance",
            VendorName = "Service B",
            CostTHB = 1000m,
            CreatedAt = DateTime.UtcNow
        };

        await _context.EquipmentMaintenanceLogs.AddRangeAsync(log1, log2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByEquipmentIdAsync(equipment.Id);

        Assert.Equal(2, result.Count);
        Assert.Equal(MaintenanceType.Corrective, result[0].Type);
        Assert.Equal(MaintenanceType.Preventive, result[1].Type);
    }

    [Fact]
    public async Task GetByEquipmentIdAsync_NoLogs_ReturnsEmptyList()
    {
        var equipment = await SeedEquipmentAsync();

        var result = await _repository.GetByEquipmentIdAsync(equipment.Id);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByEquipmentIdAsync_Limit200_ReturnsAtMost200()
    {
        var equipment = await SeedEquipmentAsync();
        var employeeId = Guid.NewGuid();

        for (int i = 0; i < 250; i++)
        {
            var log = new EquipmentMaintenanceLog
            {
                Id = Guid.NewGuid(),
                EquipmentId = equipment.Id,
                LoggedByEmployeeId = employeeId,
                Type = MaintenanceType.Preventive,
                OccurredAt = DateTime.UtcNow.AddDays(-i),
                Description = $"Maintenance {i}",
                VendorName = "Service",
                CostTHB = 100m,
                CreatedAt = DateTime.UtcNow
            };
            _context.EquipmentMaintenanceLogs.Add(log);
        }
        await _context.SaveChangesAsync();

        var result = await _repository.GetByEquipmentIdAsync(equipment.Id);

        Assert.Equal(200, result.Count);
    }
}
