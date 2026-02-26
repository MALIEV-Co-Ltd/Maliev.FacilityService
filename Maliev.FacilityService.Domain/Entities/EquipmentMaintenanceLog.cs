using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Domain.Entities;

/// <summary>
/// Represents a maintenance log entry for equipment.
/// </summary>
public class EquipmentMaintenanceLog
{
    /// <summary>
    /// Unique identifier for the maintenance log.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the equipment.
    /// </summary>
    public Guid EquipmentId { get; set; }

    /// <summary>
    /// Employee ID who logged the maintenance.
    /// </summary>
    public Guid LoggedByEmployeeId { get; set; }

    /// <summary>
    /// Date and time when the maintenance occurred (UTC).
    /// </summary>
    public DateTime OccurredAt { get; set; }

    /// <summary>
    /// Type of maintenance performed.
    /// </summary>
    public MaintenanceType Type { get; set; }

    /// <summary>
    /// Name of the vendor who performed the maintenance.
    /// </summary>
    public string? VendorName { get; set; }

    /// <summary>
    /// Description of the maintenance work.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Cost of maintenance in Thai Baht.
    /// </summary>
    public decimal? CostTHB { get; set; }

    /// <summary>
    /// Next scheduled service date.
    /// </summary>
    public DateOnly? NextServiceDueDate { get; set; }

    /// <summary>
    /// Timestamp when the log was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
