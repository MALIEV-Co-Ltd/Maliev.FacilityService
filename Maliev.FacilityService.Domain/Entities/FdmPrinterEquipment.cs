namespace Maliev.FacilityService.Domain.Entities;

/// <summary>
/// FDM (Fused Deposition Modeling) 3D printer equipment.
/// </summary>
public class FdmPrinterEquipment : ManufacturingEquipment
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
    /// Nozzle diameter in millimeters.
    /// </summary>
    public decimal NozzleDiameterMm { get; set; }

    /// <summary>
    /// Maximum nozzle temperature in Celsius.
    /// </summary>
    public decimal MaxNozzleTempC { get; set; }

    /// <summary>
    /// Number of extruders on the printer.
    /// </summary>
    public int NumberOfExtruders { get; set; }

    /// <summary>
    /// Minimum layer height in millimeters.
    /// </summary>
    public decimal MinLayerHeightMm { get; set; }

    /// <summary>
    /// Maximum layer height in millimeters.
    /// </summary>
    public decimal MaxLayerHeightMm { get; set; }
}
