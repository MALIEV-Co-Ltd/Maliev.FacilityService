using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Commands.RegisterEquipment;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Infrastructure.Data;
using Maliev.FacilityService.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Maliev.FacilityService.Tests.Integration.Commands;

[Collection("PostgresCollection")]
public class RegisterEquipmentIntegrationTests : IAsyncLifetime
{
    private readonly PostgresFixture _postgresFixture;
    private FacilityDbContext _context = null!;
    private EquipmentRepository _repository = null!;
    private RegisterEquipmentCommandHandler _handler = null!;
    private Mock<IAssetCodeGenerator> _assetCodeGeneratorMock = null!;

    public RegisterEquipmentIntegrationTests(PostgresFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
    }

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<FacilityDbContext>()
            .UseNpgsql(_postgresFixture.ConnectionString)
            .Options;

        _context = new FacilityDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        _repository = new EquipmentRepository(_context);
        _assetCodeGeneratorMock = new Mock<IAssetCodeGenerator>();
        _handler = new RegisterEquipmentCommandHandler(
            _repository,
            _assetCodeGeneratorMock.Object);
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
    public async Task HandleAsync_ValidRegistration_PersistsEquipmentToDatabase()
    {
        var expectedAssetCode = $"MAL-FDM-{Guid.NewGuid():N}".Substring(0, 12);
        _assetCodeGeneratorMock
            .Setup(x => x.GenerateAssetCodeAsync(EquipmentCategory.FdmPrinter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAssetCode);

        var command = new RegisterEquipmentCommand(
            Name: "Test FDM Printer",
            Category: EquipmentCategory.FdmPrinter,
            Brand: "Prusa",
            ModelName: "MK4",
            ManufacturerSerialNumber: "SN123456",
            SubCategory: "3D Printer",
            PurchasePriceTHB: 25000m,
            PurchaseDate: new DateOnly(2024, 1, 15),
            WarrantyExpiryDate: new DateOnly(2026, 1, 15),
            NextServiceDueDate: new DateOnly(2025, 7, 15),
            Spec: new Dictionary<string, object?>
            {
                ["BuildVolumeXMm"] = 250m,
                ["BuildVolumeYMm"] = 210m,
                ["BuildVolumeZMm"] = 220m,
                ["HourlyRateTHB"] = 500m
            });

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedAssetCode, result.AssetCode);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(EquipmentCategory.FdmPrinter, result.Category);
        Assert.Equal(EquipmentStatus.Active, result.Status);

        var persistedEquipment = await _context.Equipments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.AssetCode == expectedAssetCode);

        Assert.NotNull(persistedEquipment);
        Assert.Equal(command.Name, persistedEquipment.Name);
        Assert.Equal(command.Brand, persistedEquipment.Brand);
        Assert.Equal(command.ModelName, persistedEquipment.ModelName);
        Assert.Equal(EquipmentCategory.FdmPrinter, persistedEquipment.Category);
        Assert.Equal(EquipmentStatus.Active, persistedEquipment.Status);
    }

    [Fact]
    public async Task HandleAsync_ValidRegistrationForGeneralEquipment_PersistsEquipmentToDatabase()
    {
        var expectedAssetCode = $"MAL-OFC-{Guid.NewGuid():N}".Substring(0, 12);
        _assetCodeGeneratorMock
            .Setup(x => x.GenerateAssetCodeAsync(EquipmentCategory.OfficeEquipment, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAssetCode);

        var command = new RegisterEquipmentCommand(
            Name: "Test Office Chair",
            Category: EquipmentCategory.OfficeEquipment,
            Brand: "Herman Miller",
            ModelName: "Aeron",
            ManufacturerSerialNumber: "HM-987654",
            SubCategory: "Furniture",
            PurchasePriceTHB: 15000m,
            PurchaseDate: new DateOnly(2024, 3, 1),
            WarrantyExpiryDate: new DateOnly(2027, 3, 1),
            NextServiceDueDate: null,
            Spec: null);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedAssetCode, result.AssetCode);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(EquipmentCategory.OfficeEquipment, result.Category);

        var persistedEquipment = await _context.Equipments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.AssetCode == expectedAssetCode);

        Assert.NotNull(persistedEquipment);
        Assert.IsType<OfficeEquipmentItem>(persistedEquipment);
        Assert.Equal(command.Name, persistedEquipment.Name);
        Assert.Equal(EquipmentStatus.Active, persistedEquipment.Status);
    }

    [Fact]
    public async Task HandleAsync_MultipleRegistrations_GeneratesUniqueAssetCodes()
    {
        var callCount = 0;
        _assetCodeGeneratorMock
            .Setup(x => x.GenerateAssetCodeAsync(EquipmentCategory.FdmPrinter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => $"MAL-FDM-{++callCount:D4}");

        var command1 = new RegisterEquipmentCommand(
            Name: "First Printer",
            Category: EquipmentCategory.FdmPrinter,
            Brand: "Prusa",
            ModelName: "MK4",
            ManufacturerSerialNumber: "SN-001",
            SubCategory: null,
            PurchasePriceTHB: null,
            PurchaseDate: null,
            WarrantyExpiryDate: null,
            NextServiceDueDate: null,
            Spec: null);

        var command2 = new RegisterEquipmentCommand(
            Name: "Second Printer",
            Category: EquipmentCategory.FdmPrinter,
            Brand: "Prusa",
            ModelName: "MK4",
            ManufacturerSerialNumber: "SN-002",
            SubCategory: null,
            PurchasePriceTHB: null,
            PurchaseDate: null,
            WarrantyExpiryDate: null,
            NextServiceDueDate: null,
            Spec: null);

        var result1 = await _handler.HandleAsync(command1, CancellationToken.None);
        var result2 = await _handler.HandleAsync(command2, CancellationToken.None);

        Assert.NotNull(result1);
        Assert.NotNull(result2);
        Assert.NotEqual(result1.AssetCode, result2.AssetCode);

        var allEquipment = await _context.Equipments
            .AsNoTracking()
            .Where(e => e.Category == EquipmentCategory.FdmPrinter)
            .ToListAsync();

        Assert.Equal(2, allEquipment.Count);
    }
}
