using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Domain.Entities;

/// <summary>
/// SLA (Stereolithography) 3D printer equipment.
/// </summary>
public class SlaPrinterEquipment : ManufacturingEquipment
{
    /// <summary>
    /// Build volume in X direction in millimeters.
    /// </summary>
    public decimal BuildVolumeXMm { get; set; }

    /// <summary>
    /// Build volume in Y direction in millimeters.
    /// </summary>
    public decimal BuildVolumeYMm { get; set; }

    /// <summary>
    /// Build volume in Z direction in millimeters.
    /// </summary>
    public decimal BuildVolumeZMm { get; set; }

    /// <summary>
    /// XY resolution in millimeters.
    /// </summary>
    public decimal XyResolutionMm { get; set; }

    /// <summary>
    /// Type of light source used in the SLA printer.
    /// </summary>
    public SlaLightSourceType LightSourceType { get; set; }

    /// <summary>
    /// Light source wavelength in nanometers.
    /// </summary>
    public int WavelengthNm { get; set; }
}
