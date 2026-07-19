using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Queries.GetEquipmentLoans;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Domain.Exceptions;
using Moq;

namespace Maliev.FacilityService.Tests.Unit.Queries;

public class GetEquipmentLoansQueryHandlerTests
{
    private readonly Mock<ILoanRepository> _loanRepositoryMock;
    private readonly Mock<IEquipmentRepository> _equipmentRepositoryMock;
    private readonly GetEquipmentLoansQueryHandler _handler;

    public GetEquipmentLoansQueryHandlerTests()
    {
        _loanRepositoryMock = new Mock<ILoanRepository>();
        _equipmentRepositoryMock = new Mock<IEquipmentRepository>();
        _handler = new GetEquipmentLoansQueryHandler(
            _loanRepositoryMock.Object,
            _equipmentRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_EquipmentExists_ReturnsLoanList()
    {
        var equipmentId = Guid.NewGuid();
        var equipment = CreateTestEquipment(equipmentId);
        var loans = new List<EquipmentLoan>
        {
            CreateTestLoan(Guid.NewGuid(), equipmentId, LoanStatus.Active),
            CreateTestLoan(Guid.NewGuid(), equipmentId, LoanStatus.Returned)
        };

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _loanRepositoryMock
            .Setup(r => r.GetByEquipmentIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loans);

        var query = new GetEquipmentLoansQuery(equipmentId);
        var result = await _handler.HandleAsync(query);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task HandleAsync_EquipmentNotFound_ThrowsEquipmentNotFoundException()
    {
        var equipmentId = Guid.NewGuid();

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Equipment?)null);

        var query = new GetEquipmentLoansQuery(equipmentId);

        await Assert.ThrowsAsync<EquipmentNotFoundException>(
            () => _handler.HandleAsync(query));
    }

    [Fact]
    public async Task HandleAsync_NoLoans_ReturnsEmptyList()
    {
        var equipmentId = Guid.NewGuid();
        var equipment = CreateTestEquipment(equipmentId);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _loanRepositoryMock
            .Setup(r => r.GetByEquipmentIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<EquipmentLoan>());

        var query = new GetEquipmentLoansQuery(equipmentId);
        var result = await _handler.HandleAsync(query);

        Assert.Empty(result);
    }

    private static FdmPrinterEquipment CreateTestEquipment(Guid id)
    {
        return new FdmPrinterEquipment
        {
            Id = id,
            AssetCode = "MAL-FDM-001",
            Name = "Test FDM Printer",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.Active
        };
    }

    private static EquipmentLoan CreateTestLoan(Guid loanId, Guid equipmentId, LoanStatus status)
    {
        return new EquipmentLoan
        {
            Id = loanId,
            EquipmentId = equipmentId,
            BorrowerId = Guid.NewGuid(),
            BorrowerType = LoanBorrowerType.Employee,
            LoanStatus = status,
            LoanStartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            ExpectedReturnDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7))
        };
    }
}
