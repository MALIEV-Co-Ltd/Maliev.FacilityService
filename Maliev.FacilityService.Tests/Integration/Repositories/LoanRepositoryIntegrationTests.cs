using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Infrastructure.Data;
using Maliev.FacilityService.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Maliev.FacilityService.Tests.Integration.Repositories;

[Collection("PostgresCollection")]
public class LoanRepositoryIntegrationTests : IAsyncLifetime
{
    private readonly PostgresFixture _fixture;
    private FacilityDbContext _context = null!;
    private LoanRepository _repository = null!;

    public LoanRepositoryIntegrationTests(PostgresFixture fixture)
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

        _repository = new LoanRepository(_context);
    }

    public async Task DisposeAsync()
    {
        await _context.Database.EnsureDeletedAsync();
        await _context.DisposeAsync();
    }

    private async Task<Equipment> SeedEquipmentAsync()
    {
        var equipment = new FdmPrinterEquipment
        {
            Id = Guid.NewGuid(),
            AssetCode = "MAL-FDM-TEST-001",
            Name = "Test FDM Printer",
            Brand = "TestBrand",
            ModelName = "TestModel",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.Active,
            PurchasePriceTHB = 50000m,
            HourlyRateTHB = 200m,
            SetupFeeTHB = 50m,
            BuildVolumeXMm = 200m,
            BuildVolumeYMm = 200m,
            BuildVolumeZMm = 200m,
            NozzleDiameterMm = 0.4m,
            MaxNozzleTempC = 280m,
            NumberOfExtruders = 1,
            MinLayerHeightMm = 0.05m,
            MaxLayerHeightMm = 0.35m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Equipments.Add(equipment);
        await _context.SaveChangesAsync();
        return equipment;
    }

    [Fact]
    public async Task AddAsync_SavesLoan_ReturnsLoanWithId()
    {
        var equipment = await SeedEquipmentAsync();

        var loan = new EquipmentLoan
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            BorrowerId = Guid.NewGuid(),
            BorrowerType = LoanBorrowerType.Employee,
            LoanStatus = LoanStatus.Active,
            LoanStartDate = new DateOnly(2026, 3, 1),
            ExpectedReturnDate = new DateOnly(2026, 3, 15),
            Purpose = "Test loan"
        };

        var result = await _repository.AddAsync(loan);

        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(equipment.Id, result.EquipmentId);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingLoan_ReturnsLoan()
    {
        var equipment = await SeedEquipmentAsync();
        var loanId = Guid.NewGuid();

        var loan = new EquipmentLoan
        {
            Id = loanId,
            EquipmentId = equipment.Id,
            BorrowerId = Guid.NewGuid(),
            BorrowerType = LoanBorrowerType.Employee,
            LoanStatus = LoanStatus.Active,
            LoanStartDate = new DateOnly(2026, 3, 1),
            ExpectedReturnDate = new DateOnly(2026, 3, 15),
            Purpose = "Test loan"
        };

        await _context.EquipmentLoans.AddAsync(loan);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(loanId);

        Assert.NotNull(result);
        Assert.Equal(loanId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingLoan_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByEquipmentIdAsync_ReturnsLoansOrderedByDateDesc()
    {
        var equipment = await SeedEquipmentAsync();
        var borrowerId = Guid.NewGuid();

        var loan1 = new EquipmentLoan
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            BorrowerId = borrowerId,
            BorrowerType = LoanBorrowerType.Employee,
            LoanStatus = LoanStatus.Returned,
            LoanStartDate = new DateOnly(2026, 1, 1),
            ActualReturnDate = new DateOnly(2026, 1, 10),
            ExpectedReturnDate = new DateOnly(2026, 1, 15),
            Purpose = "First loan"
        };

        var loan2 = new EquipmentLoan
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            BorrowerId = borrowerId,
            BorrowerType = LoanBorrowerType.Employee,
            LoanStatus = LoanStatus.Active,
            LoanStartDate = new DateOnly(2026, 2, 1),
            ExpectedReturnDate = new DateOnly(2026, 2, 15),
            Purpose = "Second loan"
        };

        await _context.EquipmentLoans.AddRangeAsync(loan1, loan2);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByEquipmentIdAsync(equipment.Id);

        Assert.Equal(2, result.Count);
        Assert.Equal(LoanStatus.Active, result[0].LoanStatus);
        Assert.Equal(LoanStatus.Returned, result[1].LoanStatus);
    }

    [Fact]
    public async Task GetActiveLoanByEquipmentIdAsync_ActiveLoanExists_ReturnsLoan()
    {
        var equipment = await SeedEquipmentAsync();

        var loan = new EquipmentLoan
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            BorrowerId = Guid.NewGuid(),
            BorrowerType = LoanBorrowerType.Employee,
            LoanStatus = LoanStatus.Active,
            LoanStartDate = new DateOnly(2026, 3, 1),
            ExpectedReturnDate = new DateOnly(2026, 3, 15),
            Purpose = "Active loan"
        };

        await _context.EquipmentLoans.AddAsync(loan);
        await _context.SaveChangesAsync();

        var result = await _repository.GetActiveLoanByEquipmentIdAsync(equipment.Id);

        Assert.NotNull(result);
        Assert.Equal(LoanStatus.Active, result.LoanStatus);
    }

    [Fact]
    public async Task GetActiveLoanByEquipmentIdAsync_NoActiveLoan_ReturnsNull()
    {
        var equipment = await SeedEquipmentAsync();

        var result = await _repository.GetActiveLoanByEquipmentIdAsync(equipment.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task HasActiveLoanAsync_ActiveLoanExists_ReturnsTrue()
    {
        var equipment = await SeedEquipmentAsync();

        var loan = new EquipmentLoan
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            BorrowerId = Guid.NewGuid(),
            BorrowerType = LoanBorrowerType.Employee,
            LoanStatus = LoanStatus.Active,
            LoanStartDate = new DateOnly(2026, 3, 1),
            ExpectedReturnDate = new DateOnly(2026, 3, 15),
            Purpose = "Active loan"
        };

        await _context.EquipmentLoans.AddAsync(loan);
        await _context.SaveChangesAsync();

        var result = await _repository.HasActiveLoanAsync(equipment.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task HasActiveLoanAsync_PendingLoanExists_ReturnsTrue()
    {
        var equipment = await SeedEquipmentAsync();

        var loan = new EquipmentLoan
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            BorrowerId = Guid.NewGuid(),
            BorrowerType = LoanBorrowerType.Customer,
            LoanStatus = LoanStatus.Pending,
            LoanStartDate = new DateOnly(2026, 3, 1),
            ExpectedReturnDate = new DateOnly(2026, 3, 15),
            Purpose = "Pending loan"
        };

        await _context.EquipmentLoans.AddAsync(loan);
        await _context.SaveChangesAsync();

        var result = await _repository.HasActiveLoanAsync(equipment.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task HasActiveLoanAsync_NoActiveLoan_ReturnsFalse()
    {
        var equipment = await SeedEquipmentAsync();

        var result = await _repository.HasActiveLoanAsync(equipment.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task UpdateAsync_ExistingLoan_UpdatesSuccessfully()
    {
        var equipment = await SeedEquipmentAsync();

        var loan = new EquipmentLoan
        {
            Id = Guid.NewGuid(),
            EquipmentId = equipment.Id,
            BorrowerId = Guid.NewGuid(),
            BorrowerType = LoanBorrowerType.Employee,
            LoanStatus = LoanStatus.Active,
            LoanStartDate = new DateOnly(2026, 3, 1),
            ExpectedReturnDate = new DateOnly(2026, 3, 15),
            Purpose = "Active loan"
        };

        await _context.EquipmentLoans.AddAsync(loan);
        await _context.SaveChangesAsync();

        loan.LoanStatus = LoanStatus.Returned;
        loan.ActualReturnDate = new DateOnly(2026, 3, 15);

        var result = await _repository.UpdateAsync(loan);

        Assert.Equal(LoanStatus.Returned, result.LoanStatus);
    }
}
