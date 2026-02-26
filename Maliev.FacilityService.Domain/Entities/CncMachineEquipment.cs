using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Domain.Entities;

/// <summary>
/// CNC (Computer Numerical Control) machine equipment.
/// </summary>
public class CncMachineEquipment : ManufacturingEquipment
{
    /// <summary>
    /// Maximum travel in X direction in millimeters.
    /// </summary>
    public decimal XTravelMm { get; set; }

    /// <summary>
    /// Maximum travel in Y direction in millimeters.
    /// </summary>
    public decimal YTravelMm { get; set; }

    /// <summary>
    /// Maximum travel in Z direction in millimeters.
    /// </summary>
    public decimal ZTravelMm { get; set; }

    /// <summary>
    /// Maximum spindle speed in revolutions per minute.
    /// </summary>
    public int MaxSpindleSpeedRpm { get; set; }

    /// <summary>
    /// Maximum spindle power in kilowatts.
    /// </summary>
    public decimal MaxSpindlePowerKw { get; set; }

    /// <summary>
    /// Number of axes on the CNC machine.
    /// </summary>
    public int NumberOfAxes { get; set; }

    /// <summary>
    /// Type of tool interface.
    /// </summary>
    public CncToolInterface ToolInterface { get; set; }

    /// <summary>
    /// Maximum tool diameter in millimeters.
    /// </summary>
    public decimal MaxToolDiameterMm { get; set; }

    /// <summary>
    /// Brand of the CNC controller.
    /// </summary>
    public string? ControllerBrand { get; set; }
}
