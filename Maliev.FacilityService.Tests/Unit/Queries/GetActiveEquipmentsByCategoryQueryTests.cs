using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Queries.GetActiveEquipmentsByCategory;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Moq;

namespace Maliev.FacilityService.Tests.Unit.Queries;

public class GetActiveEquipmentsByCategoryQueryHandlerTests
{
    private readonly Mock<IEquipmentRepository> _repositoryMock;
    private readonly GetActiveEquipmentsByCategoryQueryHandler _handler;

    public GetActiveEquipmentsByCategoryQueryHandlerTests()
    {
        _repositoryMock = new Mock<IEquipmentRepository>();
        _handler = new GetActiveEquipmentsByCategoryQueryHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithSpecificCategory_ReturnsOnlyThatCategoryActiveEquipment()
    {
        var category = EquipmentCategory.FdmPrinter;
        var fdmPrinter = CreateManufacturingEquipment(category, "FDM Printer 1", EquipmentStatus.Active);
        var activeEquipment = new List<Equipment> { fdmPrinter };

        _repositoryMock
            .Setup(r => r.GetActiveByCategoryAsync(category, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeEquipment);

        var query = new GetActiveEquipmentsByCategoryQuery(category);
        var result = await _handler.HandleAsync(query);

        Assert.Single(result);
        Assert.Equal(category, result[0].Category);
        _repositoryMock.Verify(
            r => r.GetActiveByCategoryAsync(category, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithNullCategory_ReturnsAllManufacturingCategoriesActiveEquipment()
    {
        var fdmPrinter = CreateManufacturingEquipment(EquipmentCategory.FdmPrinter, "FDM Printer 1", EquipmentStatus.Active);
        var slaPrinter = CreateManufacturingEquipment(EquipmentCategory.SlaPrinter, "SLA Printer 1", EquipmentStatus.Active);
        var cncMachine = CreateManufacturingEquipment(EquipmentCategory.CncMachine, "CNC Machine 1", EquipmentStatus.Active);

        _repositoryMock
            .Setup(r => r.GetActiveByMultipleCategoriesAsync(It.IsAny<IReadOnlySet<EquipmentCategory>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Equipment> { fdmPrinter, slaPrinter, cncMachine });

        var query = new GetActiveEquipmentsByCategoryQuery(null);
        var result = await _handler.HandleAsync(query);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task HandleAsync_FiltersToManufacturingEquipmentOnly()
    {
        var manufacturingEquipment = CreateManufacturingEquipment(EquipmentCategory.FdmPrinter, "FDM Printer 1", EquipmentStatus.Active);
        var officeEquipment = CreateOfficeEquipment("Office PC 1", EquipmentStatus.Active);

        _repositoryMock
            .Setup(r => r.GetActiveByCategoryAsync(EquipmentCategory.FdmPrinter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Equipment> { manufacturingEquipment, officeEquipment });

        var query = new GetActiveEquipmentsByCategoryQuery(EquipmentCategory.FdmPrinter);
        var result = await _handler.HandleAsync(query);

        Assert.Single(result);
        Assert.Equal(EquipmentCategory.FdmPrinter, result[0].Category);
    }

    private static FdmPrinterEquipment CreateManufacturingEquipment(EquipmentCategory category, string name, EquipmentStatus status)
    {
        return new FdmPrinterEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = $"MAL-FDM-{Random.Shared.Next(1000, 9999)}",
            Name = name,
            Category = category,
            Status = status,
            HourlyRateTHB = 500m,
            SetupFeeTHB = 100m,
            BuildVolumeXMm = 200m,
            BuildVolumeYMm = 200m,
            BuildVolumeZMm = 200m,
            NozzleDiameterMm = 0.4m,
            MaxNozzleTempC = 250m,
            NumberOfExtruders = 1,
            MinLayerHeightMm = 0.1m,
            MaxLayerHeightMm = 0.3m
        };
    }

    private static Equipment CreateOfficeEquipment(string name, EquipmentStatus status)
    {
        return new TestOfficeEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = $"MAL-OFF-{Random.Shared.Next(1000, 9999)}",
            Name = name,
            Category = EquipmentCategory.OfficeEquipment,
            Status = status
        };
    }

    private class TestOfficeEquipment : Equipment
    {
    }
}
