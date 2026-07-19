using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Infrastructure.Data;
using Maliev.FacilityService.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Maliev.FacilityService.Tests.Integration.Repositories;

[Collection("PostgresCollection")]
public class EquipmentNoteRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private FacilityDbContext _context = null!;
    private EquipmentNoteRepository _repository = null!;

    public EquipmentNoteRepositoryIntegrationTests(PostgresFixture fixture)
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

        _repository = new EquipmentNoteRepository(_context);
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
    public async Task AddAsync_SavesNote_ReturnsNoteWithId()
    {
        var equipment = await SeedEquipmentAsync();

        var note = new EquipmentNote
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            AuthorEmployeeId = Guid.NewGuid(),
            Content = "This is a test note",
            CreatedAt = DateTime.UtcNow
        };

        var result = await _repository.AddAsync(note);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(equipment.Id, result.EquipmentId);
        Assert.Equal("This is a test note", result.Content);
    }

    [Fact]
    public async Task GetByEquipmentIdAsync_ReturnsNotesOrderedByDateDesc()
    {
        var equipment = await SeedEquipmentAsync();
        var employeeId = Guid.NewGuid();

        var note1 = new EquipmentNote
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            AuthorEmployeeId = employeeId,
            Content = "First note",
            CreatedAt = new DateTime(2026, 1, 1, 10, 0, 0, DateTimeKind.Utc)
        };

        var note2 = new EquipmentNote
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            AuthorEmployeeId = employeeId,
            Content = "Second note",
            CreatedAt = new DateTime(2026, 2, 1, 10, 0, 0, DateTimeKind.Utc)
        };

        await _context.EquipmentNotes.AddRangeAsync(note1, note2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByEquipmentIdAsync(equipment.Id);

        Assert.Equal(2, result.Count);
        Assert.Equal("Second note", result[0].Content);
        Assert.Equal("First note", result[1].Content);
    }

    [Fact]
    public async Task GetByEquipmentIdAsync_NoNotes_ReturnsEmptyList()
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
            var note = new EquipmentNote
            {
                Id = Guid.NewGuid(),
                EquipmentId = equipment.Id,
                AuthorEmployeeId = employeeId,
                Content = $"Note {i}",
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            };
            _context.EquipmentNotes.Add(note);
        }
        await _context.SaveChangesAsync();

        var result = await _repository.GetByEquipmentIdAsync(equipment.Id);

        Assert.Equal(200, result.Count);
    }
}
