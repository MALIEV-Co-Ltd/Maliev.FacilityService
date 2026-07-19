using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Application.DTOs;

/// <summary>
/// Full detail view of equipment, including category-specific spec data.
/// </summary>
public record EquipmentDto
{
    /// <summary>
    /// Unique identifier of the equipment.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Auto-generated asset code in format MAL-{PREFIX}-{SEQ}.
    /// </summary>
    public string AssetCode { get; init; } = string.Empty;

    /// <summary>
    /// Display name of the equipment.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// Brand or manufacturer of the equipment.
    /// </summary>
    public string? Brand { get; init; }

    /// <summary>
    /// Model name of the equipment.
    /// </summary>
    public string? ModelName { get; init; }

    /// <summary>
    /// Category of the equipment.
    /// </summary>
    public EquipmentCategory Category { get; init; }

    /// <summary>
    /// Current operational status of the equipment.
    /// </summary>
    public EquipmentStatus Status { get; init; }

    /// <summary>
    /// xmin row version used for optimistic concurrency.
    /// </summary>
    public uint RowVersion { get; init; }

    /// <summary>
    /// Purchase price in Thai Baht.
    /// </summary>
    public decimal? PurchasePriceTHB { get; init; }

    /// <summary>
    /// Timestamp when the equipment record was last updated (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; init; }

    /// <summary>
    /// Manufacturer's serial number.
    /// </summary>
    public string? ManufacturerSerialNumber { get; init; }

    /// <summary>
    /// Sub-category of the equipment.
    /// </summary>
    public string? SubCategory { get; init; }

    /// <summary>
    /// Date when the equipment was purchased.
    /// </summary>
    public DateOnly? PurchaseDate { get; init; }

    /// <summary>
    /// Warranty expiration date.
    /// </summary>
    public DateOnly? WarrantyExpiryDate { get; init; }

    /// <summary>
    /// Next scheduled service date.
    /// </summary>
    public DateOnly? NextServiceDueDate { get; init; }

    /// <summary>
    /// Timestamp when the equipment record was created (UTC).
    /// </summary>
    public DateTime CreatedAt { get; init; }

    /// <summary>
    /// Category-specific spec properties as a key-value dictionary.
    /// Null for general equipment categories that have no manufacturing specs.
    /// </summary>
    public Dictionary<string, object?>? Spec { get; init; }
}
