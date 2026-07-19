using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Commands.ReturnLoan;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Domain.Exceptions;
using Moq;

namespace Maliev.FacilityService.Tests.Unit.Commands;

public class ReturnLoanCommandHandlerTests
{
    private readonly Mock<ILoanRepository> _loanRepositoryMock;
    private readonly Mock<IEquipmentRepository> _equipmentRepositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly ReturnLoanCommandHandler _handler;

    public ReturnLoanCommandHandlerTests()
    {
        _loanRepositoryMock = new Mock<ILoanRepository>();
        _equipmentRepositoryMock = new Mock<IEquipmentRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();
        _handler = new ReturnLoanCommandHandler(
            _loanRepositoryMock.Object,
            _equipmentRepositoryMock.Object,
            _eventPublisherMock.Object);
    }

    [Fact]
    public async Task HandleAsync_ValidLoan_ReturnsUpdatedLoanWithReturnedStatus()
    {
        var loanId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var loan = CreateTestLoan(loanId, equipmentId, LoanStatus.Active);
        var equipment = CreateTestEquipment(equipmentId, EquipmentStatus.OnLoan);
        var returnDate = DateOnly.FromDateTime(DateTime.UtcNow);

        _loanRepositoryMock
            .Setup(r => r.GetByIdAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _equipmentRepositoryMock
            .Setup(r => r.UpdateAsync(equipment, It.IsAny<uint>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _loanRepositoryMock
            .Setup(r => r.UpdateAsync(loan, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        var command = new ReturnLoanCommand(loanId, returnDate, 1);
        var result = await _handler.HandleAsync(command);

        Assert.NotNull(result);
        Assert.Equal(LoanStatus.Returned, result.LoanStatus);
        _equipmentRepositoryMock.Verify(
            r => r.UpdateAsync(equipment, It.IsAny<uint>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _eventPublisherMock.Verify(
            p => p.PublishEquipmentStatusChangedAsync(
                equipmentId,
                equipment.AssetCode,
                equipment.Name,
                It.IsAny<string>(),
                EquipmentStatus.OnLoan.ToString(),
                EquipmentStatus.Active.ToString(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_LoanNotFound_ThrowsKeyNotFoundException()
    {
        var loanId = Guid.NewGuid();

        _loanRepositoryMock
            .Setup(r => r.GetByIdAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EquipmentLoan?)null);

        var command = new ReturnLoanCommand(loanId, DateOnly.FromDateTime(DateTime.UtcNow), 1);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _handler.HandleAsync(command));

        _equipmentRepositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Equipment>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_EquipmentNotFound_ThrowsEquipmentNotFoundException()
    {
        var loanId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var loan = CreateTestLoan(loanId, equipmentId, LoanStatus.Active);

        _loanRepositoryMock
            .Setup(r => r.GetByIdAsync(loanId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Equipment?)null);

        var command = new ReturnLoanCommand(loanId, DateOnly.FromDateTime(DateTime.UtcNow), 1);

        await Assert.ThrowsAsync<EquipmentNotFoundException>(
            () => _handler.HandleAsync(command));
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

    private static FdmPrinterEquipment CreateTestEquipment(Guid id, EquipmentStatus status)
    {
        return new FdmPrinterEquipment
        {
            Id = id,
            AssetCode = "MAL-FDM-001",
            Name = "Test FDM Printer",
            Category = EquipmentCategory.FdmPrinter,
            Status = status,
            BuildVolumeXMm = 200m,
            BuildVolumeYMm = 200m,
            BuildVolumeZMm = 200m
        };
    }
}
