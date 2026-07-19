using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Commands.AddAttachment;
using Maliev.FacilityService.Application.UseCases.Queries.GetAttachments;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Infrastructure.Data;
using Maliev.FacilityService.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Maliev.FacilityService.Tests.Integration.Commands;

[Collection("PostgresCollection")]
public class AddAttachmentIntegrationTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private FacilityDbContext _context = null!;
    private EquipmentRepository _equipmentRepository = null!;
    private AttachmentRepository _attachmentRepository = null!;
    private AddAttachmentCommandHandler _addHandler = null!;
    private GetAttachmentsQueryHandler _getHandler = null!;
    private Mock<IAssetCodeGenerator> _assetCodeGeneratorMock = null!;

    public AddAttachmentIntegrationTests(PostgresFixture fixture)
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

        _equipmentRepository = new EquipmentRepository(_context);
        _attachmentRepository = new AttachmentRepository(_context);
        _assetCodeGeneratorMock = new Mock<IAssetCodeGenerator>();

        _addHandler = new AddAttachmentCommandHandler(
            _equipmentRepository,
            _attachmentRepository);

        _getHandler = new GetAttachmentsQueryHandler(_attachmentRepository);
    }

    public async Task DisposeAsync()
    {
        if (_context != null)
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }
    }

    private async Task<CncMachineEquipment> CreateCncMachineAsync(string assetCode)
    {
        var equipment = new CncMachineEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = assetCode,
            Name = "Test CNC Machine",
            Category = EquipmentCategory.CncMachine,
            Status = EquipmentStatus.Active,
            Brand = "Haas",
            ModelName = "VF-2",
            ManufacturerSerialNumber = "CNC-12345",
            SubCategory = "Vertical Mill",
            PurchasePriceTHB = 1500000m,
            PurchaseDate = new DateOnly(2024, 1, 1),
            WarrantyExpiryDate = new DateOnly(2027, 1, 1),
            NextServiceDueDate = new DateOnly(2025, 7, 1),
            XTravelMm = 500m,
            YTravelMm = 400m,
            ZTravelMm = 500m,
            MaxSpindleSpeedRpm = 10000,
            MaxSpindlePowerKw = 22m,
            NumberOfAxes = 3,
            ToolInterface = CncToolInterface.Bt,
            MaxToolDiameterMm = 100m,
            ControllerBrand = "Haas"
        };

        await _equipmentRepository.AddAsync(equipment, CancellationToken.None);
        return equipment;
    }

    [Fact]
    public async Task HandleAsync_AddAttachmentToCncMachine_AttachmentIsPersisted()
    {
        var cncMachine = await CreateCncMachineAsync("MAL-CNC-0001");

        var command = new AddAttachmentCommand(
            EquipmentId: cncMachine.Id,
            Name: "4-Inch Chuck",
            AttachmentType: AttachmentType.Chuck,
            SerialNumber: "CHUCK-001",
            ConditionNotes: "Good condition");

        var result = await _addHandler.HandleAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(cncMachine.Id, result.EquipmentId);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.AttachmentType, result.AttachmentType);
        Assert.Equal(command.SerialNumber, result.SerialNumber);
        Assert.True(result.IsActive);

        var persistedAttachment = await _context.EquipmentAttachments
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == result.Id);

        Assert.NotNull(persistedAttachment);
        Assert.Equal(command.Name, persistedAttachment.Name);
        Assert.Equal(AttachmentType.Chuck, persistedAttachment.AttachmentType);
        Assert.Equal("CHUCK-001", persistedAttachment.SerialNumber);
        Assert.True(persistedAttachment.IsActive);
    }

    [Fact]
    public async Task HandleAsync_MultipleAttachments_AttachmentsReturnedInDescendingCreatedAtOrder()
    {
        var cncMachine = await CreateCncMachineAsync("MAL-CNC-0002");

        var command1 = new AddAttachmentCommand(
            EquipmentId: cncMachine.Id,
            Name: "First Tool",
            AttachmentType: AttachmentType.Tool,
            SerialNumber: "TOOL-001",
            ConditionNotes: null);

        var command2 = new AddAttachmentCommand(
            EquipmentId: cncMachine.Id,
            Name: "Second Fixture",
            AttachmentType: AttachmentType.Fixture,
            SerialNumber: "FIX-001",
            ConditionNotes: "Excellent");

        var command3 = new AddAttachmentCommand(
            EquipmentId: cncMachine.Id,
            Name: "Third Collet",
            AttachmentType: AttachmentType.Collet,
            SerialNumber: "COLLET-001",
            ConditionNotes: null);

        var result1 = await _addHandler.HandleAsync(command1, CancellationToken.None);
        await Task.Delay(10);
        var result2 = await _addHandler.HandleAsync(command2, CancellationToken.None);
        await Task.Delay(10);
        var result3 = await _addHandler.HandleAsync(command3, CancellationToken.None);

        var getQuery = new GetAttachmentsQuery(cncMachine.Id);
        var attachments = await _getHandler.HandleAsync(getQuery, CancellationToken.None);

        Assert.Equal(3, attachments.Count);
        Assert.Equal("Third Collet", attachments[0].Name);
        Assert.Equal("Second Fixture", attachments[1].Name);
        Assert.Equal("First Tool", attachments[2].Name);
    }
}
