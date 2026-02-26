using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Commands.DeleteEquipment;
using Maliev.FacilityService.Application.UseCases.Commands.RegisterEquipment;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Domain.Exceptions;
using Maliev.FacilityService.Infrastructure.Data;
using Maliev.FacilityService.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Maliev.FacilityService.Tests.Integration.Commands;

[Collection("PostgresCollection")]
public class DeleteEquipmentIntegrationTests : IAsyncLifetime
{
    private readonly PostgresFixture _postgresFixture;
    private FacilityDbContext _context = null!;
    private EquipmentRepository _repository = null!;
    private LoanRepository _loanRepository = null!;
    private DeleteEquipmentCommandHandler _handler = null!;
    private Mock<IJobServiceClient> _jobServiceClientMock = null!;
    private Mock<IAssetCodeGenerator> _assetCodeGeneratorMock = null!;

    public DeleteEquipmentIntegrationTests(PostgresFixture postgresFixture)
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
        _loanRepository = new LoanRepository(_context);
        _jobServiceClientMock = new Mock<IJobServiceClient>();
        _assetCodeGeneratorMock = new Mock<IAssetCodeGenerator>();

        _handler = new DeleteEquipmentCommandHandler(
            _repository,
            _jobServiceClientMock.Object,
            _loanRepository);
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
    public async Task HandleAsync_DeleteActiveEquipment_RemovesEquipmentFromDatabase()
    {
        var expectedAssetCode = $"MAL-FDM-{Guid.NewGuid():N}".Substring(0, 12);
        _assetCodeGeneratorMock
            .Setup(x => x.GenerateAssetCodeAsync(EquipmentCategory.FdmPrinter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAssetCode);

        var registerCommand = new RegisterEquipmentCommand(
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

        var registeredEquipment = await new RegisterEquipmentCommandHandler(
            _repository,
            _assetCodeGeneratorMock.Object).HandleAsync(registerCommand, CancellationToken.None);

        _jobServiceClientMock
            .Setup(j => j.HasHistoricalJobsAsync(registeredEquipment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var deleteCommand = new DeleteEquipmentCommand(registeredEquipment.Id);

        await _handler.HandleAsync(deleteCommand, CancellationToken.None);

        var deletedEquipment = await _context.Equipments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == registeredEquipment.Id);

        Assert.Null(deletedEquipment);
    }

    [Fact]
    public async Task HandleAsync_DeleteNonExistentEquipment_ThrowsEquipmentNotFoundException()
    {
        var nonExistentId = Guid.NewGuid();
        var command = new DeleteEquipmentCommand(nonExistentId);

        await Assert.ThrowsAsync<EquipmentNotFoundException>(
            () => _handler.HandleAsync(command, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_DeleteDecommissionedEquipment_Succeeds()
    {
        var equipment = CreateTestEquipment(EquipmentStatus.Decommissioned);
        await _context.Equipments.AddAsync(equipment);
        await _context.SaveChangesAsync();

        _jobServiceClientMock
            .Setup(j => j.HasHistoricalJobsAsync(equipment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var deleteCommand = new DeleteEquipmentCommand(equipment.Id);

        await _handler.HandleAsync(deleteCommand, CancellationToken.None);

        var deletedEquipment = await _context.Equipments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == equipment.Id);

        Assert.Null(deletedEquipment);
    }

    private static FdmPrinterEquipment CreateTestEquipment(EquipmentStatus status)
    {
        return new FdmPrinterEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = $"MAL-FDM-{Guid.NewGuid():N}".Substring(0, 12),
            Name = "Test FDM Printer Decommissioned",
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
