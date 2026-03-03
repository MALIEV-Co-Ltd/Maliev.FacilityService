using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Queries.GetEquipmentById;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Domain.Exceptions;
using Moq;

namespace Maliev.FacilityService.Tests.Unit.Queries;

public class GetEquipmentByIdQueryHandlerTests
{
    private readonly Mock<IEquipmentRepository> _repositoryMock;
    private readonly GetEquipmentByIdQueryHandler _handler;

    public GetEquipmentByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<IEquipmentRepository>();
        _handler = new GetEquipmentByIdQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_EquipmentExists_ReturnsEquipmentDto()
    {
        var equipmentId = Guid.NewGuid();
        var equipment = CreateTestEquipment(equipmentId);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        var query = new GetEquipmentByIdQuery(equipmentId);
        var result = await _handler.HandleAsync(query);

        Assert.NotNull(result);
        Assert.Equal(equipmentId, result.Id);
        Assert.Equal(equipment.Name, result.Name);
        Assert.Equal(equipment.Category, result.Category);
    }

    [Fact]
    public async Task HandleAsync_EquipmentNotFound_ThrowsEquipmentNotFoundException()
    {
        var equipmentId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Equipment?)null);

        var query = new GetEquipmentByIdQuery(equipmentId);

        await Assert.ThrowsAsync<EquipmentNotFoundException>(
            () => _handler.HandleAsync(query));
    }

    private static FdmPrinterEquipment CreateTestEquipment(Guid id)
    {
        return new FdmPrinterEquipment
        {
            Id = id,
            AssetCode = "MAL-FDM-001",
            Name = "Test FDM Printer",
            Brand = "Prusa",
            ModelName = "MK3S+",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.Active,
            BuildVolumeXMm = 250m,
            BuildVolumeYMm = 210m,
            BuildVolumeZMm = 200m,
            NozzleDiameterMm = 0.4m,
            MaxNozzleTempC = 280m,
            NumberOfExtruders = 1,
            MinLayerHeightMm = 0.1m,
            MaxLayerHeightMm = 0.3m
        };
    }
}
