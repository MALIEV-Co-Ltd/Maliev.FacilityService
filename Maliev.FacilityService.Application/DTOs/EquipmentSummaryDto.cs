using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Application.DTOs;

/// <summary>
/// Summary view of equipment for list endpoints.
/// </summary>
public record EquipmentSummaryDto
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
    /// Purchase price in Thai Baht.
    /// </summary>
    public decimal? PurchasePriceTHB { get; init; }

    /// <summary>
    /// Timestamp when the equipment record was last updated (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; init; }
}
