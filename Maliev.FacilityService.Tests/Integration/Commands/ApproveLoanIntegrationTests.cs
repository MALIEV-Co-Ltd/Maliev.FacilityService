using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Commands.ApproveLoan;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Infrastructure.Data;
using Maliev.FacilityService.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace Maliev.FacilityService.Tests.Integration.Commands;

[Collection("PostgresCollection")]
public class ApproveLoanIntegrationTests : IAsyncLifetime
{
    private readonly PostgresFixture _postgresFixture;
    private FacilityDbContext _context = null!;
    private LoanRepository _loanRepository = null!;
    private EquipmentRepository _equipmentRepository = null!;
    private Mock<IEventPublisher> _eventPublisherMock = null!;
    private ApproveLoanCommandHandler _handler = null!;

    public ApproveLoanIntegrationTests(PostgresFixture postgresFixture)
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

        _loanRepository = new LoanRepository(_context);
        _equipmentRepository = new EquipmentRepository(_context);
        _eventPublisherMock = new Mock<IEventPublisher>();
        _handler = new ApproveLoanCommandHandler(
            _loanRepository,
            _equipmentRepository,
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

    [Fact]
    public async Task HandleAsync_ApprovePendingLoan_LoanBecomesApprovedAndEquipmentStatusChangesToOnLoan()
    {
        var equipment = CreateTestEquipment(EquipmentStatus.Active);
        await _context.Equipments.AddAsync(equipment);
        await _context.SaveChangesAsync();

        var loan = CreateTestLoan(equipment.Id, LoanStatus.Pending);
        await _context.EquipmentLoans.AddAsync(loan);
        await _context.SaveChangesAsync();

        var command = new ApproveLoanCommand(
            LoanId: loan.Id,
            ApprovedByEmployeeId: Guid.NewGuid(),
            BorrowerDisplayName: "Test Customer",
            RowVersion: 1);

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(LoanStatus.Active, result.LoanStatus);

        var persistedEquipment = await _context.Equipments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == equipment.Id);

        Assert.NotNull(persistedEquipment);
        Assert.Equal(EquipmentStatus.OnLoan, persistedEquipment.Status);

        var persistedLoan = await _context.EquipmentLoans
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == loan.Id);

        Assert.NotNull(persistedLoan);
        Assert.Equal(LoanStatus.Active, persistedLoan.LoanStatus);
        Assert.NotNull(persistedLoan.ApprovedByEmployeeId);

        _eventPublisherMock.Verify(
            p => p.PublishEquipmentStatusChangedAsync(
                equipment.Id,
                equipment.AssetCode,
                equipment.Name,
                It.IsAny<string>(),
                EquipmentStatus.Active.ToString(),
                EquipmentStatus.OnLoan.ToString(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ApproveNonExistentLoan_ThrowsKeyNotFoundException()
    {
        var nonExistentLoanId = Guid.NewGuid();
        var command = new ApproveLoanCommand(
            LoanId: nonExistentLoanId,
            ApprovedByEmployeeId: Guid.NewGuid(),
            BorrowerDisplayName: "Test Borrower",
            RowVersion: 1);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.HandleAsync(command, CancellationToken.None));
    }

    private static FdmPrinterEquipment CreateTestEquipment(EquipmentStatus status)
    {
        return new FdmPrinterEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = $"MAL-FDM-{Guid.NewGuid():N}".Substring(0, 12),
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

    private static EquipmentLoan CreateTestLoan(Guid equipmentId, LoanStatus status)
    {
        return new EquipmentLoan
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipmentId,
            BorrowerId = Guid.NewGuid(),
            BorrowerType = LoanBorrowerType.Employee,
            LoanStatus = status,
            LoanStartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ExpectedReturnDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)),
            Purpose = "Test loan"
        };
    }
}
