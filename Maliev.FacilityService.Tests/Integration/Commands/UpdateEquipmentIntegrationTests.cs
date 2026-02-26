using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Commands.UpdateEquipment;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Domain.Exceptions;
using Maliev.FacilityService.Infrastructure.Data;
using Maliev.FacilityService.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Maliev.FacilityService.Tests.Integration.Commands;

[Collection("PostgresCollection")]
public class UpdateEquipmentIntegrationTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private FacilityDbContext _context = null!;

    public UpdateEquipmentIntegrationTests(PostgresFixture fixture)
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
        _context = CreateDbContext();
        await _context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        if (_context != null)
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }
    }

    [Fact]
    public async Task HandleAsync_ValidUpdate_EquipmentNameAndBrandArePersisted()
    {
        Guid equipmentId;
        using (var context1 = CreateDbContext())
        {
            var repository = new EquipmentRepository(context1);
            
            var equipment = new OfficeEquipmentItem
            {
                Id = Guid.NewGuid(),
                Name = "Original Name",
                Brand = "Original Brand",
                Category = EquipmentCategory.OfficeEquipment,
                Status = EquipmentStatus.Active,
                AssetCode = $"MAL-OFC-{Guid.NewGuid():N}".Substring(0, 12),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await repository.AddAsync(equipment);
            equipmentId = equipment.Id;
        }

        using (var context2 = CreateDbContext())
        {
            var repository2 = new EquipmentRepository(context2);
            var handler = new UpdateEquipmentCommandHandler(repository2);

            var command = new UpdateEquipmentCommand(
                EquipmentId: equipmentId,
                Name: "Updated Name",
                Brand: "Updated Brand",
                ModelName: "Model X",
                ManufacturerSerialNumber: "SN12345",
                SubCategory: "Printer",
                PurchasePriceTHB: 15000m,
                PurchaseDate: new DateOnly(2024, 1, 15),
                WarrantyExpiryDate: new DateOnly(2026, 1, 15),
                NextServiceDueDate: new DateOnly(2025, 7, 15),
                Spec: null,
                RowVersion: 0);

            var result = await handler.HandleAsync(command);

            Assert.NotNull(result);
            Assert.Equal("Updated Name", result.Name);
            Assert.Equal("Updated Brand", result.Brand);
            Assert.Equal("Model X", result.ModelName);
        }

        using var verifyContext = CreateDbContext();
        var savedEquipment = await verifyContext.Equipments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == equipmentId);

        Assert.NotNull(savedEquipment);
        Assert.Equal("Updated Name", savedEquipment.Name);
        Assert.Equal("Updated Brand", savedEquipment.Brand);
    }

    [Fact]
    public async Task HandleAsync_EquipmentNotFound_ThrowsEquipmentNotFoundException()
    {
        using var context = CreateDbContext();
        var repository = new EquipmentRepository(context);
        var handler = new UpdateEquipmentCommandHandler(repository);

        var nonExistentId = Guid.NewGuid();

        var command = new UpdateEquipmentCommand(
            EquipmentId: nonExistentId,
            Name: "Updated Name",
            Brand: "Updated Brand",
            ModelName: "Model X",
            ManufacturerSerialNumber: "SN12345",
            SubCategory: "Printer",
            PurchasePriceTHB: 15000m,
            PurchaseDate: new DateOnly(2024, 1, 15),
            WarrantyExpiryDate: new DateOnly(2026, 1, 15),
            NextServiceDueDate: new DateOnly(2025, 7, 15),
            Spec: null,
            RowVersion: 0);

        var exception = await Assert.ThrowsAsync<EquipmentNotFoundException>(
            () => handler.HandleAsync(command));

        Assert.Equal(nonExistentId, exception.EquipmentId);
    }

    [Fact]
    public async Task HandleAsync_ConcurrencyConflict_ThrowsDbUpdateConcurrencyException()
    {
        var equipmentId = Guid.NewGuid();
        var equipment = new OfficeEquipmentItem
        {
            Id = equipmentId,
            Name = "Original Name",
            Category = EquipmentCategory.OfficeEquipment,
            Status = EquipmentStatus.Active,
            AssetCode = $"MAL-OFC-{Guid.NewGuid():N}".Substring(0, 12),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // 1. Initial save
        using (var context1 = CreateDbContext())
        {
            await context1.Equipments.AddAsync(equipment);
            await context1.SaveChangesAsync();
        }

        // 2. Load the entity in context2 (this will be our "stale" context)
        using (var context2 = CreateDbContext())
        {
            var staleEquipment = await context2.Equipments.FirstAsync(e => e.Id == equipmentId);
            
            // 3. Update the entity in context3 (the "external update")
            using (var context3 = CreateDbContext())
            {
                var externalEquipment = await context3.Equipments.FirstAsync(e => e.Id == equipmentId);
                externalEquipment.Name = "External Update";
                await context3.SaveChangesAsync();
            }

            // 4. Try to save context2 - should fail because DB has newer xmin
            staleEquipment.Name = "Stale Update";
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => context2.SaveChangesAsync());
        }
    }
}
