using Maliev.FacilityService.Application.Interfaces;
using Maliev.MessagingContracts.Contracts.Facility;
using Maliev.MessagingContracts.Generated;
using MassTransit;

namespace Maliev.FacilityService.Infrastructure.Services;

/// <summary>
/// Publishes facility domain events to RabbitMQ via the MassTransit transactional outbox.
/// </summary>
public class MassTransitEventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;

    /// <summary>
    /// Initializes a new instance of <see cref="MassTransitEventPublisher"/>.
    /// </summary>
    /// <param name="publishEndpoint">The MassTransit publish endpoint.</param>
    public MassTransitEventPublisher(IPublishEndpoint publishEndpoint)
    {
        _publishEndpoint = publishEndpoint;
    }

    /// <inheritdoc />
    public async Task PublishEquipmentStatusChangedAsync(
        Guid equipmentId,
        string assetCode,
        string name,
        string category,
        string previousStatus,
        string newStatus,
        CancellationToken ct = default)
    {
        var payload = new EquipmentStatusChangedEventPayload(
            equipmentId,
            assetCode,
            name,
            category,
            previousStatus,
            newStatus);

        var message = new EquipmentStatusChangedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: "EquipmentStatusChangedEvent",
            MessageType: MessageType.Event,
            MessageVersion: "1.0",
            PublishedBy: "facility-service",
            ConsumedBy: new[] { "job-service" },
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: true,
            Payload: payload);

        await _publishEndpoint.Publish(message, ct);
    }

    /// <inheritdoc />
    public async Task PublishLoanDocumentRequestedAsync(
        Guid loanId,
        Guid equipmentId,
        string assetCode,
        string equipmentName,
        string? brand,
        string? modelName,
        string? manufacturerSerial,
        Guid borrowerId,
        string borrowerName,
        string borrowerType,
        Guid approvedByEmployeeId,
        DateOnly loanStartDate,
        DateOnly expectedReturnDate,
        string purpose,
        CancellationToken ct = default)
    {
        var payload = new LoanDocumentRequestedEventPayload(
            LoanId: loanId,
            EquipmentId: equipmentId,
            AssetCode: assetCode,
            EquipmentName: equipmentName,
            Brand: brand ?? string.Empty,
            ModelName: modelName ?? string.Empty,
            ManufacturerSerial: manufacturerSerial ?? string.Empty,
            BorrowerId: borrowerId,
            BorrowerName: borrowerName,
            BorrowerType: borrowerType,
            ApprovedByEmployeeId: approvedByEmployeeId,
            LoanStartDate: loanStartDate.ToString("yyyy-MM-dd"),
            ExpectedReturnDate: expectedReturnDate.ToString("yyyy-MM-dd"),
            Purpose: purpose,
            DocumentLanguage: "th-TH");

        var message = new LoanDocumentRequestedEvent(
            MessageId: Guid.NewGuid(),
            MessageName: "LoanDocumentRequestedEvent",
            MessageType: MessageType.Event,
            MessageVersion: "1.0",
            PublishedBy: "facility-service",
            ConsumedBy: new[] { "pdf-service" },
            CorrelationId: Guid.NewGuid(),
            CausationId: null,
            OccurredAtUtc: DateTimeOffset.UtcNow,
            IsPublic: true,
            Payload: payload);

        await _publishEndpoint.Publish(message, ct);
    }
}
