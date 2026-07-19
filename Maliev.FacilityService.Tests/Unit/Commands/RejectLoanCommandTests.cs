using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Commands.RejectLoan;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Moq;

namespace Maliev.FacilityService.Tests.Unit.Commands;

public class RejectLoanCommandHandlerTests
{
    private readonly Mock<ILoanRepository> _loanRepositoryMock;
    private readonly Mock<IEquipmentRepository> _equipmentRepositoryMock;
    private readonly RejectLoanCommandHandler _handler;

    public RejectLoanCommandHandlerTests()
    {
        _loanRepositoryMock = new Mock<ILoanRepository>();
        _equipmentRepositoryMock = new Mock<IEquipmentRepository>();
        _handler = new RejectLoanCommandHandler(
            _loanRepositoryMock.Object,
            _equipmentRepositoryMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidPendingLoan_RejectsSuccessfully()
    {
        var loanId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var loan = CreateTestLoan(loanId, equipmentId, LoanStatus.Pending);

        _loanRepositoryMock
            .Setup(r => r.GetByIdAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        _loanRepositoryMock
            .Setup(r => r.UpdateAsync(loan, It.IsAny<uint>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        var command = new RejectLoanCommand(loanId, "Test rejection reason", 1);
        var result = await _handler.HandleAsync(command);

        Assert.NotNull(result);
        Assert.Equal(LoanStatus.Rejected, result.LoanStatus);
        _loanRepositoryMock.Verify(
            r => r.UpdateAsync(loan, It.IsAny<uint>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_LoanNotFound_ThrowsKeyNotFoundException()
    {
        var loanId = Guid.NewGuid();

        _loanRepositoryMock
            .Setup(r => r.GetByIdAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EquipmentLoan?)null);

        var command = new RejectLoanCommand(loanId, "Test reason", 1);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.HandleAsync(command));

        _loanRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<EquipmentLoan>(), It.IsAny<uint>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_NonPendingLoan_ThrowsInvalidOperationException()
    {
        var loanId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var loan = CreateTestLoan(loanId, equipmentId, LoanStatus.Active);

        _loanRepositoryMock
            .Setup(r => r.GetByIdAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        var command = new RejectLoanCommand(loanId, "Test rejection reason", 1);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.HandleAsync(command));

        _loanRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<EquipmentLoan>(), It.IsAny<uint>(), It.IsAny<CancellationToken>()),
            Times.Never);
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
            ExpectedReturnDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
            Purpose = "Testing"
        };
    }
}
