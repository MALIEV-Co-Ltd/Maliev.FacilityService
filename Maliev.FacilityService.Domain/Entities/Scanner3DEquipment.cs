using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Domain.Entities;

/// <summary>
/// 3D Scanner equipment.
/// </summary>
public class Scanner3DEquipment : ManufacturingEquipment
{
    /// <summary>
    /// Maximum scan volume in cubic meters.
    /// </summary>
    public decimal MaxScanVolumeM3 { get; set; }

    /// <summary>
    /// Accuracy in millimeters.
    /// </summary>
    public decimal AccuracyMm { get; set; }

    /// <summary>
    /// Available scan resolutions stored as JSON.
    /// </summary>
    public string? ScanResolutions { get; set; }

    /// <summary>
    /// Scanner technology type.
    /// </summary>
    public Scanner3DTechnology ScannerTechnology { get; set; }
}
