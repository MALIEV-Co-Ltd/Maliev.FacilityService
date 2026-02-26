using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Commands.UpdateEquipment;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Maliev.FacilityService.Tests.Unit.Commands;

public class UpdateEquipmentCommandHandlerTests
{
    private readonly Mock<IEquipmentRepository> _mockRepository;
    private readonly UpdateEquipmentCommandHandler _handler;

    public UpdateEquipmentCommandHandlerTests()
    {
        _mockRepository = new Mock<IEquipmentRepository>();
        _handler = new UpdateEquipmentCommandHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidUpdate_SavesChangesAndReturnsDto()
    {
        var equipmentId = Guid.NewGuid();
        var command = new UpdateEquipmentCommand(
            EquipmentId: equipmentId,
            Name: "Updated Printer",
            Brand: "Prusa",
            ModelName: "MK4",
            ManufacturerSerialNumber: "SN12345",
            SubCategory: "3D Printer",
            PurchasePriceTHB: 15000m,
            PurchaseDate: new DateOnly(2024, 1, 15),
            WarrantyExpiryDate: new DateOnly(2026, 1, 15),
            NextServiceDueDate: new DateOnly(2025, 7, 15),
            Spec: null,
            RowVersion: 1);

        var existingEquipment = new OfficeEquipmentItem
        {
            Id = equipmentId,
            Name = "Old Printer",
            Category = EquipmentCategory.OfficeEquipment,
            Status = EquipmentStatus.Active,
            AssetCode = "MAL-OFC-001",
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddDays(-10)
        };

        var updatedEquipment = new OfficeEquipmentItem
        {
            Id = equipmentId,
            Name = command.Name,
            Brand = command.Brand,
            ModelName = command.ModelName,
            ManufacturerSerialNumber = command.ManufacturerSerialNumber,
            SubCategory = command.SubCategory,
            PurchasePriceTHB = command.PurchasePriceTHB,
            PurchaseDate = command.PurchaseDate,
            WarrantyExpiryDate = command.WarrantyExpiryDate,
            NextServiceDueDate = command.NextServiceDueDate,
            Category = EquipmentCategory.OfficeEquipment,
            Status = EquipmentStatus.Active,
            AssetCode = "MAL-OFC-001",
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEquipment);

        _mockRepository
            .Setup(r => r.UpdateAsync(existingEquipment, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedEquipment);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.Brand, result.Brand);
        Assert.Equal(command.ModelName, result.ModelName);
        Assert.Equal(command.ManufacturerSerialNumber, result.ManufacturerSerialNumber);
        Assert.Equal(command.SubCategory, result.SubCategory);
        Assert.Equal(command.PurchasePriceTHB, result.PurchasePriceTHB);
        Assert.Equal(command.PurchaseDate, result.PurchaseDate);
        Assert.Equal(command.WarrantyExpiryDate, result.WarrantyExpiryDate);
        Assert.Equal(command.NextServiceDueDate, result.NextServiceDueDate);

        _mockRepository.Verify(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(existingEquipment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_EquipmentNotFound_ThrowsEquipmentNotFoundException()
    {
        var equipmentId = Guid.NewGuid();
        var command = new UpdateEquipmentCommand(
            EquipmentId: equipmentId,
            Name: "Updated Printer",
            Brand: "Prusa",
            ModelName: "MK4",
            ManufacturerSerialNumber: "SN12345",
            SubCategory: "3D Printer",
            PurchasePriceTHB: 15000m,
            PurchaseDate: new DateOnly(2024, 1, 15),
            WarrantyExpiryDate: new DateOnly(2026, 1, 15),
            NextServiceDueDate: new DateOnly(2025, 7, 15),
            Spec: null,
            RowVersion: 1);

        _mockRepository
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Equipment?)null);

        var exception = await Assert.ThrowsAsync<EquipmentNotFoundException>(
            () => _handler.HandleAsync(command, CancellationToken.None));

        Assert.Equal(equipmentId, exception.EquipmentId);

        _mockRepository.Verify(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(It.IsAny<Equipment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ConcurrencyConflict_ThrowsDbUpdateConcurrencyException()
    {
        var equipmentId = Guid.NewGuid();
        var command = new UpdateEquipmentCommand(
            EquipmentId: equipmentId,
            Name: "Updated Printer",
            Brand: "Prusa",
            ModelName: "MK4",
            ManufacturerSerialNumber: "SN12345",
            SubCategory: "3D Printer",
            PurchasePriceTHB: 15000m,
            PurchaseDate: new DateOnly(2024, 1, 15),
            WarrantyExpiryDate: new DateOnly(2026, 1, 15),
            NextServiceDueDate: new DateOnly(2025, 7, 15),
            Spec: null,
            RowVersion: 1);

        var existingEquipment = new OfficeEquipmentItem
        {
            Id = equipmentId,
            Name = "Old Printer",
            Category = EquipmentCategory.OfficeEquipment,
            Status = EquipmentStatus.Active,
            AssetCode = "MAL-OFC-001",
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            UpdatedAt = DateTime.UtcNow.AddDays(-10)
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingEquipment);

        _mockRepository
            .Setup(r => r.UpdateAsync(existingEquipment, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException("Concurrency conflict"));

        var exception = await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => _handler.HandleAsync(command, CancellationToken.None));

        _mockRepository.Verify(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(existingEquipment, It.IsAny<CancellationToken>()), Times.Once);
    }
}
