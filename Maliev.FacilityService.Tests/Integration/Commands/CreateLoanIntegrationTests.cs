using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Commands.CreateLoan;
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
public class CreateLoanIntegrationTests : IAsyncLifetime
{
    private readonly PostgresFixture _postgresFixture;
    private FacilityDbContext _context = null!;
    private EquipmentRepository _equipmentRepository = null!;
    private LoanRepository _loanRepository = null!;
    private CreateLoanCommandHandler _handler = null!;
    private Mock<IEventPublisher> _eventPublisherMock = null!;
    private Mock<IAssetCodeGenerator> _assetCodeGeneratorMock = null!;

    public CreateLoanIntegrationTests(PostgresFixture postgresFixture)
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

        _equipmentRepository = new EquipmentRepository(_context);
        _loanRepository = new LoanRepository(_context);
        _eventPublisherMock = new Mock<IEventPublisher>();
        _assetCodeGeneratorMock = new Mock<IAssetCodeGenerator>();

        _handler = new CreateLoanCommandHandler(
            _equipmentRepository,
            _loanRepository,
            _eventPublisherMock.Object);
    }

    public async Task DisposeAsync()
    {
        if (_context != null)
        {
            await _context.Database.EnsureDeletedAsync();
            await _context.DisposeAsync();
        }
    }

    private FacilityDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<FacilityDbContext>()
            .UseNpgsql(_postgresFixture.ConnectionString)
            .Options;
        return new FacilityDbContext(options);
    }

    private async Task<Equipment> CreateTestEquipmentAsync(EquipmentRepository repository, EquipmentCategory category, string? assetCode = null)
    {
        var actualAssetCode = assetCode ?? $"MAL-{category}-{Guid.NewGuid():N}".Substring(0, 12);
        _assetCodeGeneratorMock
            .Setup(x => x.GenerateAssetCodeAsync(category, It.IsAny<CancellationToken>()))
            .ReturnsAsync(actualAssetCode);

        var registerCommand = new RegisterEquipmentCommand(
            Name: "Test Equipment",
            Category: category,
            Brand: "TestBrand",
            ModelName: "TestModel",
            ManufacturerSerialNumber: "SN-TEST-001",
            SubCategory: "Test",
            PurchasePriceTHB: 10000m,
            PurchaseDate: new DateOnly(2024, 1, 1),
            WarrantyExpiryDate: new DateOnly(2026, 1, 1),
            NextServiceDueDate: new DateOnly(2025, 6, 1),
            Spec: null);

        var registerHandler = new RegisterEquipmentCommandHandler(
            repository,
            _assetCodeGeneratorMock.Object);

        var result = await registerHandler.HandleAsync(registerCommand, CancellationToken.None);

        return await repository.GetByIdAsync(result.Id, CancellationToken.None)
            ?? throw new InvalidOperationException("Failed to retrieve created equipment");
    }

    [Fact]
    public async Task HandleAsync_EmployeeLoan_ImmediatelyActiveAndEquipmentStatusChangesToOnLoan()
    {
        var equipment = await CreateTestEquipmentInNewContextAsync(EquipmentCategory.FdmPrinter, "MAL-FDM-TEST-001");

        await using var context = CreateDbContext();
        var equipmentRepo = new EquipmentRepository(context);
        var loanRepo = new LoanRepository(context);
        var handler = new CreateLoanCommandHandler(equipmentRepo, loanRepo, _eventPublisherMock.Object);

        var borrowerId = Guid.NewGuid();

        var command = new CreateLoanCommand(
            EquipmentId: equipment.Id,
            BorrowerId: borrowerId,
            BorrowerType: LoanBorrowerType.Employee,
            LoanStartDate: new DateOnly(2026, 3, 1),
            ExpectedReturnDate: new DateOnly(2026, 3, 15),
            Purpose: "Project work");

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(LoanStatus.Active, result.LoanStatus);
        Assert.Equal(equipment.Id, result.EquipmentId);
        Assert.Equal(borrowerId, result.BorrowerId);

        await using var verifyContext = CreateDbContext();
        var updatedEquipment = await verifyContext.Equipments
            .AsNoTracking()
            .FirstAsync(e => e.Id == equipment.Id);

        Assert.Equal(EquipmentStatus.OnLoan, updatedEquipment.Status);

        _eventPublisherMock.Verify(
            x => x.PublishEquipmentStatusChangedAsync(
                equipment.Id,
                equipment.AssetCode,
                It.IsAny<string>(),
                It.IsAny<string>(),
                EquipmentStatus.Active.ToString(),
                EquipmentStatus.OnLoan.ToString(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private async Task<Equipment> CreateTestEquipmentInNewContextAsync(EquipmentCategory category, string? assetCode = null)
    {
        await using var context = CreateDbContext();
        var repository = new EquipmentRepository(context);
        return await CreateTestEquipmentAsync(repository, category, assetCode);
    }

    [Fact]
    public async Task HandleAsync_CustomerLoan_RemainsPendingAndEquipmentStatusNotChanged()
    {
        var equipment = await CreateTestEquipmentInNewContextAsync(EquipmentCategory.SlaPrinter, "MAL-SLA-TEST-001");

        await using var context = CreateDbContext();
        var equipmentRepo = new EquipmentRepository(context);
        var loanRepo = new LoanRepository(context);
        var handler = new CreateLoanCommandHandler(equipmentRepo, loanRepo, _eventPublisherMock.Object);

        var borrowerId = Guid.NewGuid();

        var command = new CreateLoanCommand(
            EquipmentId: equipment.Id,
            BorrowerId: borrowerId,
            BorrowerType: LoanBorrowerType.Customer,
            LoanStartDate: new DateOnly(2026, 3, 1),
            ExpectedReturnDate: new DateOnly(2026, 3, 15),
            Purpose: "Customer demonstration");

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(LoanStatus.Pending, result.LoanStatus);
        Assert.Equal(equipment.Id, result.EquipmentId);
        Assert.Equal(borrowerId, result.BorrowerId);

        await using var verifyContext = CreateDbContext();
        var updatedEquipment = await verifyContext.Equipments
            .AsNoTracking()
            .FirstAsync(e => e.Id == equipment.Id);

        Assert.Equal(EquipmentStatus.Active, updatedEquipment.Status);

        _eventPublisherMock.Verify(
            x => x.PublishEquipmentStatusChangedAsync(
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
    public async Task HandleAsync_EquipmentAlreadyOnLoan_ThrowsLoanNotAllowedException()
    {
        var equipment = await CreateTestEquipmentInNewContextAsync(EquipmentCategory.CncMachine, "MAL-CNC-TEST-001");

        // First loan - create in a separate context
        await using (var context1 = CreateDbContext())
        {
            var equipmentRepo1 = new EquipmentRepository(context1);
            var loanRepo1 = new LoanRepository(context1);
            var handler1 = new CreateLoanCommandHandler(equipmentRepo1, loanRepo1, _eventPublisherMock.Object);

            var firstLoanCommand = new CreateLoanCommand(
                EquipmentId: equipment.Id,
                BorrowerId: Guid.NewGuid(),
                BorrowerType: LoanBorrowerType.Employee,
                LoanStartDate: new DateOnly(2026, 3, 1),
                ExpectedReturnDate: new DateOnly(2026, 3, 15),
                Purpose: "First loan");

            await handler1.HandleAsync(firstLoanCommand, CancellationToken.None);
        }

        // Second loan attempt - should fail
        await using var context2 = CreateDbContext();
        var equipmentRepo2 = new EquipmentRepository(context2);
        var loanRepo2 = new LoanRepository(context2);
        var handler2 = new CreateLoanCommandHandler(equipmentRepo2, loanRepo2, _eventPublisherMock.Object);

        var secondLoanCommand = new CreateLoanCommand(
            EquipmentId: equipment.Id,
            BorrowerId: Guid.NewGuid(),
            BorrowerType: LoanBorrowerType.Employee,
            LoanStartDate: new DateOnly(2026, 4, 1),
            ExpectedReturnDate: new DateOnly(2026, 4, 15),
            Purpose: "Second loan attempt");

        var exception = await Assert.ThrowsAsync<LoanNotAllowedException>(
            () => handler2.HandleAsync(secondLoanCommand, CancellationToken.None));

        Assert.Equal(equipment.Id, exception.EquipmentId);
        Assert.Contains("already has an active loan", exception.Message, StringComparison.OrdinalIgnoreCase);
    }
}
