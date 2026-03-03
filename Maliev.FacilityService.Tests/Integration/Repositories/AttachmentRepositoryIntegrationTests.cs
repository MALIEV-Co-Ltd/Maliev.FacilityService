using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Infrastructure.Data;
using Maliev.FacilityService.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Maliev.FacilityService.Tests.Integration.Repositories;

[Collection("PostgresCollection")]
public class AttachmentRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private FacilityDbContext _context = null!;
    private AttachmentRepository _repository = null!;

    public AttachmentRepositoryIntegrationTests(PostgresFixture fixture)
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

        _repository = new AttachmentRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    private async Task<Equipment> SeedEquipmentAsync()
    {
        var equipment = new CncMachineEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-CNC-TEST-001",
            Name = "Test CNC Machine",
            Brand = "TestBrand",
            ModelName = "TestModel",
            Category = EquipmentCategory.CncMachine,
            Status = EquipmentStatus.Active,
            PurchasePriceTHB = 150000m,
            HourlyRateTHB = 500m,
            SetupFeeTHB = 100m,
            XTravelMm = 500m,
            YTravelMm = 500m,
            ZTravelMm = 200m,
            MaxSpindleSpeedRpm = 20000,
            MaxSpindlePowerKw = 2.0m,
            NumberOfAxes = 3,
            ToolInterface = CncToolInterface.Iso,
            MaxToolDiameterMm = 10m,
            ControllerBrand = "TestController",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Equipments.Add(equipment);
        await _context.SaveChangesAsync();
        return equipment;
    }

    [Fact]
    public async Task AddAsync_SavesAttachment_ReturnsAttachmentWithId()
    {
        var equipment = await SeedEquipmentAsync();

        var attachment = new EquipmentAttachment
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            Name = "Test End Mill",
            AttachmentType = AttachmentType.Tool,
            SerialNumber = "SN-ATT-001",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _repository.AddAsync(attachment);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(equipment.Id, result.EquipmentId);
        Assert.Equal("Test End Mill", result.Name);
    }

    [Fact]
    public async Task GetByEquipmentIdAsync_ReturnsAttachmentsOrderedByDateDesc()
    {
        var equipment = await SeedEquipmentAsync();

        var attachment1 = new EquipmentAttachment
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            Name = "First Attachment",
            AttachmentType = AttachmentType.Tool,
            SerialNumber = "SN-001",
            IsActive = true,
            CreatedAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc)
        };

        var attachment2 = new EquipmentAttachment
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            Name = "Second Attachment",
            AttachmentType = AttachmentType.Fixture,
            SerialNumber = "SN-002",
            IsActive = true,
            CreatedAt = new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc),
            UpdatedAt = new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc)
        };

        await _context.EquipmentAttachments.AddRangeAsync(attachment1, attachment2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByEquipmentIdAsync(equipment.Id);

        Assert.Equal(2, result.Count);
        Assert.Equal("Second Attachment", result[0].Name);
        Assert.Equal("First Attachment", result[1].Name);
    }

    [Fact]
    public async Task GetActiveByEquipmentIdAsync_OnlyReturnsActiveAttachments()
    {
        var equipment = await SeedEquipmentAsync();

        var attachment1 = new EquipmentAttachment
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            Name = "Active Attachment",
            AttachmentType = AttachmentType.Tool,
            SerialNumber = "SN-001",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var attachment2 = new EquipmentAttachment
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            Name = "Inactive Attachment",
            AttachmentType = AttachmentType.Tool,
            SerialNumber = "SN-002",
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.EquipmentAttachments.AddRangeAsync(attachment1, attachment2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetActiveByEquipmentIdAsync(equipment.Id);

        Assert.Single(result);
        Assert.Equal("Active Attachment", result[0].Name);
    }

    [Fact]
    public async Task GetByEquipmentIdAsync_NoAttachments_ReturnsEmptyList()
    {
        var equipment = await SeedEquipmentAsync();

        var result = await _repository.GetByEquipmentIdAsync(equipment.Id);

        Assert.Empty(result);
    }

    [Fact]
    public async Task UpdateAsync_ExistingAttachment_UpdatesSuccessfully()
    {
        var equipment = await SeedEquipmentAsync();

        var attachment = new EquipmentAttachment
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            Name = "Original Name",
            AttachmentType = AttachmentType.Tool,
            SerialNumber = "SN-001",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.EquipmentAttachments.AddAsync(attachment);
        await _context.SaveChangesAsync();

        attachment.Name = "Updated Name";
        attachment.IsActive = false;

        var result = await _repository.UpdateAsync(attachment);

        Assert.Equal("Updated Name", result.Name);
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task DeleteAsync_ExistingAttachment_ReturnsTrue()
    {
        var equipment = await SeedEquipmentAsync();

        var attachment = new EquipmentAttachment
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            Name = "To Be Deleted",
            AttachmentType = AttachmentType.Tool,
            SerialNumber = "SN-001",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.EquipmentAttachments.AddAsync(attachment);
        await _context.SaveChangesAsync();

        var result = await _repository.DeleteAsync(attachment.Id);

        Assert.True(result);

        var deleted = await _context.EquipmentAttachments.FindAsync(attachment.Id);
        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingAttachment_ReturnsFalse()
    {
        var result = await _repository.DeleteAsync(Guid.NewGuid());
        Assert.False(result);
    }

    [Fact]
    public async Task GetByEquipmentIdAsync_Limit200_ReturnsAtMost200()
    {
        var equipment = await SeedEquipmentAsync();

        for (int i = 0; i < 250; i++)
        {
            var attachment = new EquipmentAttachment
            {
                Id = Guid.NewGuid(),
                EquipmentId = equipment.Id,
                Name = $"Attachment {i}",
                AttachmentType = AttachmentType.Tool,
                SerialNumber = $"SN-{i:D3}",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.EquipmentAttachments.Add(attachment);
        }
        await _context.SaveChangesAsync();

        var result = await _repository.GetByEquipmentIdAsync(equipment.Id);

        Assert.Equal(200, result.Count);
    }
}
