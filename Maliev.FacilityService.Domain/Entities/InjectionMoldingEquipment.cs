namespace Maliev.FacilityService.Domain.Entities;

/// <summary>
/// Injection molding equipment for plastic manufacturing.
/// </summary>
public class InjectionMoldingEquipment : ManufacturingEquipment
{
    /// <summary>
    /// Maximum mold size in X direction in millimeters.
    /// </summary>
    public decimal MaxMoldXMm { get; set; }

    /// <summary>
    /// Maximum mold size in Y direction in millimeters.
    /// </summary>
    public decimal MaxMoldYMm { get; set; }

    /// <summary>
    /// Maximum mold size in Z direction in millimeters.
    /// </summary>
    public decimal MaxMoldZMm { get; set; }

    /// <summary>
    /// Maximum shot size in grams.
    /// </summary>
    public decimal MaxShotSizeG { get; set; }

    /// <summary>
    /// Maximum barrel temperature in Celsius.
    /// </summary>
    public int MaxTempC { get; set; }

    /// <summary>
    /// Maximum injection pressure in bar.
    /// </summary>
    public int MaxInjectionPressureBar { get; set; }
}
