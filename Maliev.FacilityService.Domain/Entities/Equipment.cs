using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Domain.Exceptions;

namespace Maliev.FacilityService.Domain.Entities;

/// <summary>
/// Abstract base entity for all equipment in the facility management system.
/// </summary>
public abstract class Equipment
{
    /// <summary>
    /// Valid status transitions for equipment.
    /// </summary>
    private static readonly Dictionary<EquipmentStatus, IReadOnlySet<EquipmentStatus>> ValidTransitions = new()
    {
        [EquipmentStatus.Active] = new HashSet<EquipmentStatus>
        {
            EquipmentStatus.UnderMaintenance,
            EquipmentStatus.OnLoan,
            EquipmentStatus.Lost,
            EquipmentStatus.Decommissioned
        },
        [EquipmentStatus.UnderMaintenance] = new HashSet<EquipmentStatus>
        {
            EquipmentStatus.Active,
            EquipmentStatus.Decommissioned
        },
        [EquipmentStatus.OnLoan] = new HashSet<EquipmentStatus>
        {
            EquipmentStatus.Active,
            EquipmentStatus.Lost,
            EquipmentStatus.Decommissioned
        },
        [EquipmentStatus.Lost] = new HashSet<EquipmentStatus>
        {
            EquipmentStatus.Active,
            EquipmentStatus.Decommissioned
        },
        [EquipmentStatus.Decommissioned] = new HashSet<EquipmentStatus>()
    };

    /// <summary>
    /// Transitions equipment to a new status, validating the transition is allowed.
    /// </summary>
    /// <param name="newStatus">The target status to transition to.</param>
    /// <exception cref="InvalidStatusTransitionException">Thrown if the transition is not permitted.</exception>
    public void TransitionTo(EquipmentStatus newStatus)
    {
        if (!ValidTransitions.TryGetValue(Status, out var allowed) || !allowed.Contains(newStatus))
            throw new InvalidStatusTransitionException(Status.ToString(), newStatus.ToString());
        Status = newStatus;
    }

    /// <summary>
    /// Unique identifier for the equipment.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Auto-generated asset code in format MAL-{PREFIX}-{SEQ}.
    /// </summary>
    public string AssetCode { get; set; } = string.Empty;

    /// <summary>
    /// Brand or manufacturer of the equipment.
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Model name of the equipment.
    /// </summary>
    public string? ModelName { get; set; }

    /// <summary>
    /// Display name for the equipment (required, unique).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Manufacturer's serial number.
    /// </summary>
    public string? ManufacturerSerialNumber { get; set; }

    /// <summary>
    /// Category of the equipment.
    /// </summary>
    public EquipmentCategory Category { get; set; }

    /// <summary>
    /// Sub-category of the equipment.
    /// </summary>
    public string? SubCategory { get; set; }

    /// <summary>
    /// Current operational status of the equipment.
    /// </summary>
    public EquipmentStatus Status { get; set; }

    /// <summary>
    /// Date when the equipment was purchased.
    /// </summary>
    public DateOnly? PurchaseDate { get; set; }

    /// <summary>
    /// Purchase price in Thai Baht.
    /// </summary>
    public decimal? PurchasePriceTHB { get; set; }

    /// <summary>
    /// Warranty expiration date.
    /// </summary>
    public DateOnly? WarrantyExpiryDate { get; set; }

    /// <summary>
    /// Next scheduled service date.
    /// </summary>
    public DateOnly? NextServiceDueDate { get; set; }

    /// <summary>
    /// Timestamp when the equipment record was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Timestamp when the equipment record was last updated (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
