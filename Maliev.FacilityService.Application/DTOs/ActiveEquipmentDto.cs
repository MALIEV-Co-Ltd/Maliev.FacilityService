using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Application.DTOs;

/// <summary>
/// Lightweight DTO for active equipment, used by PricingService and JobService queries.
/// </summary>
public record ActiveEquipmentDto
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
    /// Category of the equipment.
    /// </summary>
    public EquipmentCategory Category { get; init; }

    /// <summary>
    /// Hourly usage rate in Thai Baht.
    /// </summary>
    public decimal HourlyRateTHB { get; init; }

    /// <summary>
    /// Setup fee in Thai Baht.
    /// </summary>
    public decimal SetupFeeTHB { get; init; }

    /// <summary>
    /// Indicates whether the equipment is outsourced.
    /// Always false — reserved for future use.
    /// </summary>
    public bool IsOutsourced { get; init; }
}
