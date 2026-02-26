namespace Maliev.FacilityService.Domain.Entities;

/// <summary>
/// Abstract base entity for manufacturing equipment (FDM, SLA, CNC, 3D Scanner, Injection Molding).
/// </summary>
public abstract class ManufacturingEquipment : Equipment
{
    /// <summary>
    /// Hourly rate for using the equipment in Thai Baht.
    /// </summary>
    public decimal HourlyRateTHB { get; set; }

    /// <summary>
    /// Setup fee in Thai Baht.
    /// </summary>
    public decimal SetupFeeTHB { get; set; }

    /// <summary>
    /// Extended properties stored as JSON (machine-specific settings).
    /// </summary>
    public string? ExtendedProperties { get; set; }
}
