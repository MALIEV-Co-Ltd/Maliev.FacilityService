using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Commands.ChangeEquipmentStatus;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Domain.Exceptions;
using Moq;

namespace Maliev.FacilityService.Tests.Unit.Commands;

public class ChangeEquipmentStatusCommandHandlerTests
{
    private readonly Mock<IEquipmentRepository> _equipmentRepositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly ChangeEquipmentStatusCommandHandler _handler;

    public ChangeEquipmentStatusCommandHandlerTests()
    {
        _equipmentRepositoryMock = new Mock<IEquipmentRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _handler = new ChangeEquipmentStatusCommandHandler(
            _equipmentRepositoryMock.Object,
            _eventPublisherMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidStatusTransition_ReturnsUpdatedEquipment()
    {
        var equipmentId = Guid.NewGuid();
        var equipment = CreateTestEquipment(equipmentId, EquipmentStatus.Active);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _equipmentRepositoryMock
            .Setup(r => r.UpdateAsync(equipment, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        var command = new ChangeEquipmentStatusCommand(
            equipmentId,
            EquipmentStatus.UnderMaintenance,
            "Routine maintenance",
            1);

        var result = await _handler.HandleAsync(command);

        Assert.NotNull(result);
        Assert.Equal(EquipmentStatus.UnderMaintenance, result.Status);
        _equipmentRepositoryMock.Verify(
            r => r.UpdateAsync(equipment, It.IsAny<CancellationToken>()),
            Times.Once);
        _eventPublisherMock.Verify(
            p => p.PublishEquipmentStatusChangedAsync(
                equipmentId,
                equipment.AssetCode,
                equipment.Name,
                It.IsAny<string>(),
                EquipmentStatus.Active.ToString(),
                EquipmentStatus.UnderMaintenance.ToString(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_InvalidStatusTransition_ThrowsInvalidStatusTransitionException()
    {
        var equipmentId = Guid.NewGuid();
        var equipment = CreateTestEquipment(equipmentId, EquipmentStatus.Decommissioned);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        var command = new ChangeEquipmentStatusCommand(
            equipmentId,
            EquipmentStatus.Active,
            "Attempting to reactivate",
            1);

        await Assert.ThrowsAsync<InvalidStatusTransitionException>(
            () => _handler.HandleAsync(command));

        _equipmentRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Equipment>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _eventPublisherMock.Verify(
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
    public async Task HandleAsync_EquipmentNotFound_ThrowsEquipmentNotFoundException()
    {
        var equipmentId = Guid.NewGuid();

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Equipment?)null);

        var command = new ChangeEquipmentStatusCommand(
            equipmentId,
            EquipmentStatus.Active,
            null,
            1);

        await Assert.ThrowsAsync<EquipmentNotFoundException>(
            () => _handler.HandleAsync(command));

        _equipmentRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Equipment>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _eventPublisherMock.Verify(
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

    private static FdmPrinterEquipment CreateTestEquipment(Guid id, EquipmentStatus status)
    {
        return new FdmPrinterEquipment
        {
            Id = id,
            AssetCode = "MAL-FDM-001",
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
