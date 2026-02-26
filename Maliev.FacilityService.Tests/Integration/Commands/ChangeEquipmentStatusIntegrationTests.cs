using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Commands.ChangeEquipmentStatus;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Domain.Exceptions;
using Maliev.FacilityService.Infrastructure.Data;
using Maliev.FacilityService.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Maliev.FacilityService.Tests.Integration.Commands;

[Collection("PostgresCollection")]
public class ChangeEquipmentStatusIntegrationTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private FacilityDbContext _dbContext = null!;

    public ChangeEquipmentStatusIntegrationTests(PostgresFixture fixture)
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

    [Fact]
    public async Task HandleAsync_ValidStatusTransition_UpdatesStatusInDatabase()
    {
        using var context = CreateDbContext();
        var repository = new EquipmentRepository(context);
        var eventPublisherMock = new Mock<IEventPublisher>();
        var handler = new ChangeEquipmentStatusCommandHandler(repository, eventPublisherMock.Object);

        var equipment = CreateTestEquipment(EquipmentStatus.Active);
        await context.Equipments.AddAsync(equipment);
        await context.SaveChangesAsync();

        var command = new ChangeEquipmentStatusCommand(
            equipment.Id,
            EquipmentStatus.UnderMaintenance,
            "Routine maintenance",
            1);

        var result = await handler.HandleAsync(command);

        Assert.NotNull(result);
        Assert.Equal(EquipmentStatus.UnderMaintenance, result.Status);

        using var verifyContext = CreateDbContext();
        var persistedEquipment = await verifyContext.Equipments.FindAsync(equipment.Id);
        Assert.NotNull(persistedEquipment);
        Assert.Equal(EquipmentStatus.UnderMaintenance, persistedEquipment.Status);

        eventPublisherMock.Verify(
            p => p.PublishEquipmentStatusChangedAsync(
                equipment.Id,
                equipment.AssetCode,
                equipment.Name,
                It.IsAny<string>(),
                EquipmentStatus.Active.ToString(),
                EquipmentStatus.UnderMaintenance.ToString(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_InvalidStatusTransition_ThrowsExceptionAndDoesNotPersist()
    {
        using var context = CreateDbContext();
        var repository = new EquipmentRepository(context);
        var eventPublisherMock = new Mock<IEventPublisher>();
        var handler = new ChangeEquipmentStatusCommandHandler(repository, eventPublisherMock.Object);

        var equipment = CreateTestEquipment(EquipmentStatus.Decommissioned);
        await context.Equipments.AddAsync(equipment);
        await context.SaveChangesAsync();

        var command = new ChangeEquipmentStatusCommand(
            equipment.Id,
            EquipmentStatus.Active,
            "Attempting to reactivate",
            1);

        await Assert.ThrowsAsync<InvalidStatusTransitionException>(
            () => handler.HandleAsync(command));

        using var verifyContext = CreateDbContext();
        var persistedEquipment = await verifyContext.Equipments.FindAsync(equipment.Id);
        Assert.NotNull(persistedEquipment);
        Assert.Equal(EquipmentStatus.Decommissioned, persistedEquipment.Status);

        eventPublisherMock.Verify(
            p => p.PublishEquipmentStatusChangedAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_StatusChange_PersistedInDatabase()
    {
        using var context = CreateDbContext();
        var repository = new EquipmentRepository(context);
        var eventPublisherMock = new Mock<IEventPublisher>();
        var handler = new ChangeEquipmentStatusCommandHandler(repository, eventPublisherMock.Object);

        var equipment = CreateTestEquipment(EquipmentStatus.Active);
        await context.Equipments.AddAsync(equipment);
        await context.SaveChangesAsync();

        var command = new ChangeEquipmentStatusCommand(
            equipment.Id,
            EquipmentStatus.OnLoan,
            "Loan to employee",
            1);

        await handler.HandleAsync(command);

        using var verifyContext = CreateDbContext();
        var persistedEquipment = await verifyContext.Equipments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == equipment.Id);

        Assert.NotNull(persistedEquipment);
        Assert.Equal(EquipmentStatus.OnLoan, persistedEquipment.Status);
        Assert.True(persistedEquipment.UpdatedAt >= equipment.UpdatedAt.AddSeconds(-1));
    }

    private static FdmPrinterEquipment CreateTestEquipment(EquipmentStatus status)
    {
        return new FdmPrinterEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = $"MAL-FDM-{Guid.NewGuid():N}".Substring(0, 12),
            Name = "Test FDM Printer",
            Brand = "TestBrand",
            ModelName = "Model X",
            Category = EquipmentCategory.FdmPrinter,
            Status = status,
            BuildVolumeXMm = 200m,
            BuildVolumeYMm = 200m,
            BuildVolumeZMm = 200m,
            NozzleDiameterMm = 0.4m,
            MaxNozzleTempC = 280m,
            NumberOfExtruders = 1,
            MinLayerHeightMm = 0.1m,
            MaxLayerHeightMm = 0.3m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
