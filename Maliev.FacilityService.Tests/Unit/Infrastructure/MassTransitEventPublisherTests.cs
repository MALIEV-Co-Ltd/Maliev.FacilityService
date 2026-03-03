using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Infrastructure.Services;
using MassTransit;
using Moq;

namespace Maliev.FacilityService.Tests.Unit.Infrastructure;

public class MassTransitEventPublisherTests
{
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly MassTransitEventPublisher _publisher;

    public MassTransitEventPublisherTests()
    {
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _publisher = new MassTransitEventPublisher(_publishEndpointMock.Object);
    }

    [Fact]
    public async Task PublishEquipmentStatusChangedAsync_CallsPublishEndpoint()
    {
        var equipmentId = Guid.NewGuid();
        var assetCode = "MAL-FDM-001";
        var name = "Test Printer";
        var category = "FdmPrinter";
        var previousStatus = "Active";
        var newStatus = "UnderMaintenance";

        await _publisher.PublishEquipmentStatusChangedAsync(
            equipmentId,
            assetCode,
            name,
            category,
            previousStatus,
            newStatus);

        _publishEndpointMock.Verify(
            p => p.Publish(
                It.IsAny<Maliev.MessagingContracts.Contracts.Facility.EquipmentStatusChangedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishLoanDocumentRequestedAsync_CallsPublishEndpoint()
    {
        var loanId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var returnDate = startDate.AddDays(7);

        await _publisher.PublishLoanDocumentRequestedAsync(
            loanId,
            equipmentId,
            "MAL-CNC-001",
            "Test CNC",
            "Haas",
            "VF2",
            "SN-12345",
            borrowerId,
            "John Doe",
            "Customer",
            employeeId,
            startDate,
            returnDate,
            "Testing");

        _publishEndpointMock.Verify(
            p => p.Publish(
                It.IsAny<Maliev.MessagingContracts.Contracts.Facility.LoanDocumentRequestedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PublishLoanDocumentRequestedAsync_NullOptionalFields_HandlesGracefully()
    {
        var loanId = Guid.NewGuid();
        var equipmentId = Guid.NewGuid();
        var borrowerId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var returnDate = startDate.AddDays(7);

        await _publisher.PublishLoanDocumentRequestedAsync(
            loanId,
            equipmentId,
            "MAL-CNC-001",
            "Test CNC",
            null,
            null,
            null,
            borrowerId,
            "John Doe",
            "Employee",
            employeeId,
            startDate,
            returnDate,
            "Testing");

        _publishEndpointMock.Verify(
            p => p.Publish(
                It.IsAny<Maliev.MessagingContracts.Contracts.Facility.LoanDocumentRequestedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
