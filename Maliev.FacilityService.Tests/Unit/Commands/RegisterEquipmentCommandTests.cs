using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Commands.RegisterEquipment;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Moq;

namespace Maliev.FacilityService.Tests.Unit.Commands;

public class RegisterEquipmentCommandHandlerTests
{
    private readonly Mock<IEquipmentRepository> _equipmentRepositoryMock;
    private readonly Mock<IAssetCodeGenerator> _assetCodeGeneratorMock;
    private readonly RegisterEquipmentCommandHandler _handler;

    public RegisterEquipmentCommandHandlerTests()
    {
        _equipmentRepositoryMock = new Mock<IEquipmentRepository>();
        _assetCodeGeneratorMock = new Mock<IAssetCodeGenerator>();
        _handler = new RegisterEquipmentCommandHandler(
            _equipmentRepositoryMock.Object,
            _assetCodeGeneratorMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidRegistration_ReturnsEquipmentWithGeneratedAssetCode()
    {
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

        var expectedAssetCode = "MAL-FDM-0001";
        _assetCodeGeneratorMock
            .Setup(x => x.GenerateAssetCodeAsync(EquipmentCategory.FdmPrinter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAssetCode);

        var savedEquipment = new FdmPrinterEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = expectedAssetCode,
            Name = command.Name,
            Brand = command.Brand,
            ModelName = command.ModelName,
            ManufacturerSerialNumber = command.ManufacturerSerialNumber,
            Category = command.Category,
            SubCategory = command.SubCategory,
            Status = EquipmentStatus.Active,
            PurchaseDate = command.PurchaseDate,
            PurchasePriceTHB = command.PurchasePriceTHB,
            WarrantyExpiryDate = command.WarrantyExpiryDate,
            NextServiceDueDate = command.NextServiceDueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _equipmentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Equipment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedEquipment);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedAssetCode, result.AssetCode);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.Category, result.Category);
        Assert.Equal(EquipmentStatus.Active, result.Status);

        _assetCodeGeneratorMock.Verify(
            x => x.GenerateAssetCodeAsync(EquipmentCategory.FdmPrinter, It.IsAny<CancellationToken>()),
            Times.Once);

        _equipmentRepositoryMock.Verify(
            x => x.AddAsync(It.IsAny<Equipment>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ValidRegistrationForGeneralEquipment_ReturnsEquipmentWithGeneratedAssetCode()
    {
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

        var expectedAssetCode = "MAL-OFC-0001";
        _assetCodeGeneratorMock
            .Setup(x => x.GenerateAssetCodeAsync(EquipmentCategory.OfficeEquipment, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAssetCode);

        var savedEquipment = new OfficeEquipmentItem
        {
            Id = Guid.NewGuid(),
            AssetCode = expectedAssetCode,
            Name = command.Name,
            Brand = command.Brand,
            ModelName = command.ModelName,
            ManufacturerSerialNumber = command.ManufacturerSerialNumber,
            Category = command.Category,
            SubCategory = command.SubCategory,
            Status = EquipmentStatus.Active,
            PurchaseDate = command.PurchaseDate,
            PurchasePriceTHB = command.PurchasePriceTHB,
            WarrantyExpiryDate = command.WarrantyExpiryDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _equipmentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Equipment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(savedEquipment);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(expectedAssetCode, result.AssetCode);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(EquipmentCategory.OfficeEquipment, result.Category);
    }

    [Fact]
    public async Task HandleAsync_DuplicateSerialNumber_ThrowsException()
    {
        var command = new RegisterEquipmentCommand(
            Name: "Duplicate Serial Number Printer",
            Category: EquipmentCategory.FdmPrinter,
            Brand: "Prusa",
            ModelName: "MK4",
            ManufacturerSerialNumber: "DUPLICATE-SN",
            SubCategory: null,
            PurchasePriceTHB: 25000m,
            PurchaseDate: null,
            WarrantyExpiryDate: null,
            NextServiceDueDate: null,
            Spec: null);

        var expectedAssetCode = "MAL-FDM-0002";
        _assetCodeGeneratorMock
            .Setup(x => x.GenerateAssetCodeAsync(EquipmentCategory.FdmPrinter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAssetCode);

        _equipmentRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<Equipment>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Equipment with this serial number already exists."));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.HandleAsync(command, CancellationToken.None));

        Assert.Contains("serial number", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
