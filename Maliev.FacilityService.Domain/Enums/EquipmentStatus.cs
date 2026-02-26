namespace Maliev.FacilityService.Domain.Enums;

/// <summary>
/// Represents the operational status of equipment.
/// </summary>
public enum EquipmentStatus
{
    Active,
    UnderMaintenance,
    OnLoan,
    Lost,
    Decommissioned
}
