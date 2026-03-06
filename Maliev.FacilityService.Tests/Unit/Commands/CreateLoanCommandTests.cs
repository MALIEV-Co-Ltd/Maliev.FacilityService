using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Commands.ApproveLoan;
using Maliev.FacilityService.Application.UseCases.Commands.CreateLoan;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Domain.Exceptions;
using Moq;

namespace Maliev.FacilityService.Tests.Unit.Commands;

public class CreateLoanCommandTests
{
    private readonly Mock<IEquipmentRepository> _equipmentRepositoryMock;
    private readonly Mock<ILoanRepository> _loanRepositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly CreateLoanCommandHandler _createHandler;
    private readonly ApproveLoanCommandHandler _approveHandler;

    public CreateLoanCommandTests()
    {
        _equipmentRepositoryMock = new Mock<IEquipmentRepository>();
        _loanRepositoryMock = new Mock<ILoanRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();

        _createHandler = new CreateLoanCommandHandler(
            _equipmentRepositoryMock.Object,
            _loanRepositoryMock.Object,
            _eventPublisherMock.Object);

        _approveHandler = new ApproveLoanCommandHandler(
            _loanRepositoryMock.Object,
            _equipmentRepositoryMock.Object,
            _eventPublisherMock.Object);
    }

    #region CreateLoan Tests

    [Fact]
    public async Task CreateLoan_ValidEmployeeLoan_ReturnsActiveLoan()
    {
        var equipmentId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();

        var command = new CreateLoanCommand(
            EquipmentId: equipmentId,
            BorrowerId: borrowerId,
            BorrowerType: LoanBorrowerType.Employee,
            LoanStartDate: new DateOnly(2026, 3, 1),
            ExpectedReturnDate: new DateOnly(2026, 3, 15),
            Purpose: "Project work");

        var equipment = new FdmPrinterEquipment
        {
            Id = equipmentId,
            AssetCode = "MAL-FDM-0001",
            Name = "Test Printer",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.Active,
            Brand = "Prusa",
            ModelName = "MK4",
            ManufacturerSerialNumber = "SN123",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _equipmentRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _loanRepositoryMock
            .Setup(x => x.HasActiveLoanAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _equipmentRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Equipment>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _loanRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<EquipmentLoan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EquipmentLoan loan, CancellationToken _) => loan);

        var result = await _createHandler.HandleAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(LoanStatus.Active, result.LoanStatus);
        Assert.Equal(equipmentId, result.EquipmentId);
        Assert.Equal(borrowerId, result.BorrowerId);

        _equipmentRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Equipment>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _eventPublisherMock.Verify(
            x => x.PublishEquipmentStatusChangedAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateLoan_ValidCustomerLoan_ReturnsPendingLoan()
    {
        var equipmentId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();

        var command = new CreateLoanCommand(
            EquipmentId: equipmentId,
            BorrowerId: borrowerId,
            BorrowerType: LoanBorrowerType.Customer,
            LoanStartDate: new DateOnly(2026, 3, 1),
            ExpectedReturnDate: new DateOnly(2026, 3, 15),
            Purpose: "Customer demonstration");

        var equipment = new FdmPrinterEquipment
        {
            Id = equipmentId,
            AssetCode = "MAL-FDM-0002",
            Name = "Demo Printer",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.Active,
            Brand = "Prusa",
            ModelName = "MK4",
            ManufacturerSerialNumber = "SN456",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _equipmentRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _loanRepositoryMock
            .Setup(x => x.HasActiveLoanAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _loanRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<EquipmentLoan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EquipmentLoan loan, CancellationToken _) => loan);

        var result = await _createHandler.HandleAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(LoanStatus.Pending, result.LoanStatus);
        Assert.Equal(equipmentId, result.EquipmentId);
        Assert.Equal(borrowerId, result.BorrowerId);

        _equipmentRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Equipment>(), It.IsAny<CancellationToken>()),
            Times.Never);

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
    public async Task CreateLoan_EquipmentAlreadyOnLoan_ThrowsLoanNotAllowedException()
    {
        var equipmentId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();

        var command = new CreateLoanCommand(
            EquipmentId: equipmentId,
            BorrowerId: borrowerId,
            BorrowerType: LoanBorrowerType.Employee,
            LoanStartDate: new DateOnly(2026, 3, 1),
            ExpectedReturnDate: new DateOnly(2026, 3, 15),
            Purpose: "Project work");

        var equipment = new FdmPrinterEquipment
        {
            Id = equipmentId,
            AssetCode = "MAL-FDM-0003",
            Name = "On Loan Printer",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.OnLoan,
            Brand = "Prusa",
            ModelName = "MK4",
            ManufacturerSerialNumber = "SN789",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _equipmentRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _loanRepositoryMock
            .Setup(x => x.HasActiveLoanAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<LoanNotAllowedException>(
            () => _createHandler.HandleAsync(command, CancellationToken.None));

        Assert.Equal(equipmentId, exception.EquipmentId);
        Assert.Contains("already has an active loan", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateLoan_EquipmentNotFound_ThrowsEquipmentNotFoundException()
    {
        var equipmentId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();

        var command = new CreateLoanCommand(
            EquipmentId: equipmentId,
            BorrowerId: borrowerId,
            BorrowerType: LoanBorrowerType.Employee,
            LoanStartDate: new DateOnly(2026, 3, 1),
            ExpectedReturnDate: new DateOnly(2026, 3, 15),
            Purpose: "Project work");

        _equipmentRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Equipment?)null);

        var exception = await Assert.ThrowsAsync<EquipmentNotFoundException>(
            () => _createHandler.HandleAsync(command, CancellationToken.None));

        Assert.Equal(equipmentId, exception.EquipmentId);
    }

    #endregion

    #region ApproveLoan Tests

    [Fact]
    public async Task ApproveLoan_ValidApproval_ReturnsApprovedLoanAndPublishesEvent()
    {
        var loanId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var approvedByEmployeeId = Guid.NewGuid();

        var command = new ApproveLoanCommand(
            LoanId: loanId,
            ApprovedByEmployeeId: approvedByEmployeeId,
            BorrowerDisplayName: "Test Employee",
            EquipmentRowVersion: 1,
            LoanRowVersion: 1);

        var loan = new EquipmentLoan
        {
            Id = loanId,
            EquipmentId = equipmentId,
            BorrowerId = borrowerId,
            BorrowerType = LoanBorrowerType.Employee,
            LoanStatus = LoanStatus.Pending,
            LoanStartDate = new DateOnly(2026, 3, 1),
            ExpectedReturnDate = new DateOnly(2026, 3, 15),
            Purpose = "Project work"
        };

        var equipment = new FdmPrinterEquipment
        {
            Id = equipmentId,
            AssetCode = "MAL-FDM-0004",
            Name = "Approved Printer",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.Active,
            Brand = "Prusa",
            ModelName = "MK4",
            ManufacturerSerialNumber = "SN999",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _loanRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        _equipmentRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _equipmentRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Equipment>(), It.IsAny<uint>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _loanRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<EquipmentLoan>(), It.IsAny<uint>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        var result = await _approveHandler.HandleAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(LoanStatus.Active, result.LoanStatus);
        Assert.Equal(approvedByEmployeeId, loan.ApprovedByEmployeeId);

        _equipmentRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<Equipment>(), It.IsAny<uint>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _loanRepositoryMock.Verify(
            x => x.UpdateAsync(It.IsAny<EquipmentLoan>(), It.IsAny<uint>(), It.IsAny<CancellationToken>()),
            Times.Once);

        _eventPublisherMock.Verify(
            x => x.PublishEquipmentStatusChangedAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _eventPublisherMock.Verify(
            x => x.PublishLoanDocumentRequestedAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<DateOnly>(),
                It.IsAny<DateOnly>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ApproveLoan_CustomerLoan_PublishesLoanDocumentRequestedEvent()
    {
        var loanId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var approvedByEmployeeId = Guid.NewGuid();

        var command = new ApproveLoanCommand(
            LoanId: loanId,
            ApprovedByEmployeeId: approvedByEmployeeId,
            BorrowerDisplayName: "Test Customer",
            EquipmentRowVersion: 1,
            LoanRowVersion: 1);

        var loan = new EquipmentLoan
        {
            Id = loanId,
            EquipmentId = equipmentId,
            BorrowerId = borrowerId,
            BorrowerType = LoanBorrowerType.Customer,
            LoanStatus = LoanStatus.Pending,
            LoanStartDate = new DateOnly(2026, 3, 1),
            ExpectedReturnDate = new DateOnly(2026, 3, 15),
            Purpose = "Customer demo"
        };

        var equipment = new FdmPrinterEquipment
        {
            Id = equipmentId,
            AssetCode = "MAL-FDM-0005",
            Name = "Customer Demo Printer",
            Category = EquipmentCategory.FdmPrinter,
            Status = EquipmentStatus.Active,
            Brand = "Prusa",
            ModelName = "MK4",
            ManufacturerSerialNumber = "SN888",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _loanRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        _equipmentRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _equipmentRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<Equipment>(), It.IsAny<uint>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _loanRepositoryMock
            .Setup(x => x.UpdateAsync(It.IsAny<EquipmentLoan>(), It.IsAny<uint>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        var result = await _approveHandler.HandleAsync(command, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(LoanStatus.Active, result.LoanStatus);

        _eventPublisherMock.Verify(
            x => x.PublishLoanDocumentRequestedAsync(
                loanId,
                equipmentId,
                equipment.AssetCode,
                equipment.Name,
                equipment.Brand,
                equipment.ModelName,
                equipment.ManufacturerSerialNumber,
                borrowerId,
                "Test Customer",
                LoanBorrowerType.Customer.ToString(),
                approvedByEmployeeId,
                loan.LoanStartDate,
                loan.ExpectedReturnDate,
                loan.Purpose,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ApproveLoan_LoanNotFound_ThrowsKeyNotFoundException()
    {
        var loanId = Guid.NewGuid();
        var approvedByEmployeeId = Guid.NewGuid();

        var command = new ApproveLoanCommand(
            LoanId: loanId,
            ApprovedByEmployeeId: approvedByEmployeeId,
            BorrowerDisplayName: "Test Borrower",
            EquipmentRowVersion: 1,
            LoanRowVersion: 1);

        _loanRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EquipmentLoan?)null);

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _approveHandler.HandleAsync(command, CancellationToken.None));

        Assert.Contains(loanId.ToString(), exception.Message);
    }

    [Fact]
    public async Task ApproveLoan_EquipmentNotFound_ThrowsEquipmentNotFoundException()
    {
        var loanId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var approvedByEmployeeId = Guid.NewGuid();

        var command = new ApproveLoanCommand(
            LoanId: loanId,
            ApprovedByEmployeeId: approvedByEmployeeId,
            BorrowerDisplayName: "Test Borrower",
            EquipmentRowVersion: 1,
            LoanRowVersion: 1);

        var loan = new EquipmentLoan
        {
            Id = loanId,
            EquipmentId = equipmentId,
            BorrowerId = Guid.NewGuid(),
            BorrowerType = LoanBorrowerType.Employee,
            LoanStatus = LoanStatus.Pending,
            LoanStartDate = new DateOnly(2026, 3, 1),
            ExpectedReturnDate = new DateOnly(2026, 3, 15),
            Purpose = "Project work"
        };

        _loanRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        _equipmentRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Equipment?)null);

        var exception = await Assert.ThrowsAsync<EquipmentNotFoundException>(
            () => _approveHandler.HandleAsync(command, CancellationToken.None));

        Assert.Equal(equipmentId, exception.EquipmentId);
    }

    #endregion
}
