using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Commands.AddMaintenanceLog;
using Maliev.FacilityService.Application.UseCases.Queries.GetMaintenanceLogs;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Maliev.FacilityService.Tests.Integration.Commands;

[Collection("PostgresCollection")]
public class AddMaintenanceLogIntegrationTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private FacilityDbContext _dbContext = null!;

    public AddMaintenanceLogIntegrationTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    private FacilityDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<FacilityDbContext>()
            .UseNpgsql(_fixture.ConnectionString)
            .Options;
        return new FacilityDbContext(options);
    }

    public async Task InitializeAsync()
    {
        _dbContext = CreateDbContext();
        await _dbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.DisposeAsync();
        }
    }

    private async Task<FdmPrinterEquipment> CreateTestEquipmentAsync()
    {
        using var context = CreateDbContext();
        var equipment = new FdmPrinterEquipment
        {
            Id = Guid.NewGuid(),
            Name = "Test FDM Printer",
            AssetCode = $"MAL-TST-{Guid.NewGuid():N}".Substring(0, 12),
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.Active,
            BuildVolumeXMm = 200,
            BuildVolumeYMm = 200,
            BuildVolumeZMm = 200,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var repo = new Infrastructure.Data.Repositories.EquipmentRepository(context);
        await repo.AddAsync(equipment);
        return equipment;
    }

    [Fact]
    public async Task AddMaintenanceLog_LogIsPersistedWithCorrectTimestamps()
    {
        var testEquipment = await CreateTestEquipmentAsync();

        using var context = CreateDbContext();
        var equipmentRepository = new Infrastructure.Data.Repositories.EquipmentRepository(context);
        var maintenanceLogRepository = new Infrastructure.Data.Repositories.MaintenanceLogRepository(context);

        var command = new AddMaintenanceLogCommand(
            testEquipment.Id,
            MaintenanceType.Preventive,
            "Replaced filament extruder",
            DateTime.UtcNow.AddHours(-2),
            Guid.NewGuid(),
            "3D Printer Service Co.",
            1500.00m,
            DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3)));

        var handler = new AddMaintenanceLogCommandHandler(equipmentRepository, maintenanceLogRepository);

        var result = await handler.HandleAsync(command);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(testEquipment.Id, result.EquipmentId);
        Assert.Equal(MaintenanceType.Preventive, result.Type);
        Assert.Equal("Replaced filament extruder", result.Description);
        Assert.Equal("3D Printer Service Co.", result.VendorName);
        Assert.Equal(1500.00m, result.CostTHB);
        Assert.True(result.CreatedAt <= DateTime.UtcNow);
        Assert.True(result.CreatedAt > DateTime.UtcNow.AddSeconds(-5));
        Assert.Equal(command.OccurredAt, result.OccurredAt);
        Assert.Equal(command.LoggedByEmployeeId, result.LoggedByEmployeeId);
        Assert.Equal(command.NextServiceDueDate, result.NextServiceDueDate);
    }

    [Fact]
    public async Task RetrieveMaintenanceLogsForEquipment_LogsReturnedInDescendingOrderByDate()
    {
        var testEquipment = await CreateTestEquipmentAsync();

        using var context = CreateDbContext();
        var maintenanceLogRepository = new Infrastructure.Data.Repositories.MaintenanceLogRepository(context);

        var employeeId1 = Guid.NewGuid();
        var employeeId2 = Guid.NewGuid();
        var employeeId3 = Guid.NewGuid();

        var log1 = await maintenanceLogRepository.AddAsync(new EquipmentMaintenanceLog
        {
            Id = Guid.NewGuid(),
            EquipmentId = testEquipment.Id,
            Type = MaintenanceType.Preventive,
            Description = "First maintenance",
            OccurredAt = DateTime.UtcNow.AddHours(-2),
            LoggedByEmployeeId = employeeId1,
            CreatedAt = DateTime.UtcNow.AddHours(-2)
        }, CancellationToken.None);

        var log2 = await maintenanceLogRepository.AddAsync(new EquipmentMaintenanceLog
        {
            Id = Guid.NewGuid(),
            EquipmentId = testEquipment.Id,
            Type = MaintenanceType.Corrective,
            Description = "Second maintenance",
            OccurredAt = DateTime.UtcNow.AddHours(-1),
            LoggedByEmployeeId = employeeId2,
            CreatedAt = DateTime.UtcNow.AddHours(-1)
        }, CancellationToken.None);

        var log3 = await maintenanceLogRepository.AddAsync(new EquipmentMaintenanceLog
        {
            Id = Guid.NewGuid(),
            EquipmentId = testEquipment.Id,
            Type = MaintenanceType.Inspection,
            Description = "Third maintenance",
            OccurredAt = DateTime.UtcNow,
            LoggedByEmployeeId = employeeId3,
            CreatedAt = DateTime.UtcNow
        }, CancellationToken.None);

        var query = new GetMaintenanceLogsQuery(testEquipment.Id);
        var handler = new GetMaintenanceLogsQueryHandler(maintenanceLogRepository);

        var result = await handler.HandleAsync(query);

        Assert.Equal(3, result.Count);
        Assert.Equal("Third maintenance", result[0].Description);
        Assert.Equal(MaintenanceType.Inspection, result[0].Type);
        Assert.Equal("Second maintenance", result[1].Description);
        Assert.Equal(MaintenanceType.Corrective, result[1].Type);
        Assert.Equal("First maintenance", result[2].Description);
        Assert.Equal(MaintenanceType.Preventive, result[2].Type);
    }
}
