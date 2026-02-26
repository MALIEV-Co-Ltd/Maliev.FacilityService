using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Application.DTOs;

/// <summary>
/// DTO representing an equipment maintenance log entry.
/// </summary>
public record MaintenanceLogDto
{
    /// <summary>
    /// Unique identifier of the maintenance log entry.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// ID of the associated equipment.
    /// </summary>
    public Guid EquipmentId { get; init; }

    /// <summary>
    /// Type of maintenance performed.
    /// </summary>
    public MaintenanceType Type { get; init; }

    /// <summary>
    /// Description of the maintenance work.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Date and time when the maintenance occurred (UTC).
    /// </summary>
    public DateTime OccurredAt { get; init; }

    /// <summary>
    /// Employee ID who logged the maintenance.
    /// </summary>
    public Guid LoggedByEmployeeId { get; init; }

    /// <summary>
    /// Name of the vendor who performed the maintenance.
    /// </summary>
    public string? VendorName { get; init; }

    /// <summary>
    /// Cost of maintenance in Thai Baht.
    /// </summary>
    public decimal? CostTHB { get; init; }

    /// <summary>
    /// Next scheduled service date.
    /// </summary>
    public DateOnly? NextServiceDueDate { get; init; }

    /// <summary>
    /// Timestamp when the log entry was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; init; }
}
