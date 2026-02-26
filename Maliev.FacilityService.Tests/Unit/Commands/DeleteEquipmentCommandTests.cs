using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.UseCases.Commands.DeleteEquipment;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Domain.Exceptions;
using Moq;
using Xunit;

namespace Maliev.FacilityService.Tests.Unit.Commands;

/// <summary>
/// Unit tests for <see cref="DeleteEquipmentCommandHandler"/>.
/// </summary>
public class DeleteEquipmentCommandHandlerTests
{
    private readonly Mock<IEquipmentRepository> _equipmentRepositoryMock;
    private readonly Mock<ILoanRepository> _loanRepositoryMock;
    private readonly Mock<IJobServiceClient> _jobServiceClientMock;
    private readonly DeleteEquipmentCommandHandler _handler;

    public DeleteEquipmentCommandHandlerTests()
    {
        _equipmentRepositoryMock = new Mock<IEquipmentRepository>();
        _loanRepositoryMock = new Mock<ILoanRepository>();
        _jobServiceClientMock = new Mock<IJobServiceClient>();

        _handler = new DeleteEquipmentCommandHandler(
            _equipmentRepositoryMock.Object,
            _jobServiceClientMock.Object,
            _loanRepositoryMock.Object);
    }

    /// <summary>
    /// Scenario 1: Valid delete of non-terminal status equipment succeeds.
    /// </summary>
    [Fact]
    public async Task HandleAsync_ValidEquipment_DeletesSuccessfully()
    {
        var equipmentId = Guid.NewGuid();
        var equipment = CreateTestEquipment(equipmentId, EquipmentStatus.Active);
        var command = new DeleteEquipmentCommand(equipmentId);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _loanRepositoryMock
            .Setup(r => r.HasActiveLoanAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _jobServiceClientMock
            .Setup(j => j.HasHistoricalJobsAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _equipmentRepositoryMock
            .Setup(r => r.DeleteAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _handler.HandleAsync(command);

        _equipmentRepositoryMock.Verify(
            r => r.DeleteAsync(equipmentId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    /// <summary>
    /// Scenario 2: Equipment not found throws EquipmentNotFoundException.
    /// </summary>
    [Fact]
    public async Task HandleAsync_EquipmentNotFound_ThrowsEquipmentNotFoundException()
    {
        var equipmentId = Guid.NewGuid();
        var command = new DeleteEquipmentCommand(equipmentId);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Equipment?)null);

        await Assert.ThrowsAsync<EquipmentNotFoundException>(
            () => _handler.HandleAsync(command));

        _equipmentRepositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Scenario 3: Equipment with active loan throws LoanNotAllowedException.
    /// </summary>
    [Fact]
    public async Task HandleAsync_EquipmentWithActiveLoan_ThrowsLoanNotAllowedException()
    {
        var equipmentId = Guid.NewGuid();
        var equipment = CreateTestEquipment(equipmentId, EquipmentStatus.OnLoan);
        var command = new DeleteEquipmentCommand(equipmentId);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _loanRepositoryMock
            .Setup(r => r.HasActiveLoanAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<LoanNotAllowedException>(
            () => _handler.HandleAsync(command));

        Assert.Equal(equipmentId, exception.EquipmentId);

        _equipmentRepositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Scenario 4: Equipment with job history throws EquipmentHasJobHistoryException.
    /// </summary>
    [Fact]
    public async Task HandleAsync_EquipmentWithJobHistory_ThrowsEquipmentHasJobHistoryException()
    {
        var equipmentId = Guid.NewGuid();
        var equipment = CreateTestEquipment(equipmentId, EquipmentStatus.Active);
        var command = new DeleteEquipmentCommand(equipmentId);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _loanRepositoryMock
            .Setup(r => r.HasActiveLoanAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _jobServiceClientMock
            .Setup(j => j.HasHistoricalJobsAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var exception = await Assert.ThrowsAsync<EquipmentHasJobHistoryException>(
            () => _handler.HandleAsync(command));

        Assert.Equal(equipmentId, exception.EquipmentId);

        _equipmentRepositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    /// <summary>
    /// Scenario 5: Terminal status (Decommissioned) equipment can be deleted.
    /// </summary>
    [Fact]
    public async Task HandleAsync_DecommissionedEquipment_DeletesSuccessfully()
    {
        var equipmentId = Guid.NewGuid();
        var equipment = CreateTestEquipment(equipmentId, EquipmentStatus.Decommissioned);
        var command = new DeleteEquipmentCommand(equipmentId);

        _equipmentRepositoryMock
            .Setup(r => r.GetByIdAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(equipment);

        _loanRepositoryMock
            .Setup(r => r.HasActiveLoanAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _jobServiceClientMock
            .Setup(j => j.HasHistoricalJobsAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _equipmentRepositoryMock
            .Setup(r => r.DeleteAsync(equipmentId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _handler.HandleAsync(command);

        _equipmentRepositoryMock.Verify(
            r => r.DeleteAsync(equipmentId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static Equipment CreateTestEquipment(Guid id, EquipmentStatus status)
    {
        return new TestEquipment
        {
            Id = id,
            Name = "Test Equipment",
            AssetCode = "MAL-TST-001",
            Category = EquipmentCategory.Other,
            Status = status,
            Brand = "TestBrand",
            ModelName = "TestModel"
        };
    }

    private sealed class TestEquipment : Equipment
    {
    }
}
