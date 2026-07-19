namespace Maliev.FacilityService.Domain.Authorization;

/// <summary>
/// Defines all permission constants for the Facility Service.
/// </summary>
public static class FacilityPermissions
{
    /// <summary>
    /// Permission to read equipment information.
    /// </summary>
    public const string EquipmentsRead = "facility.equipments.read";

    /// <summary>
    /// Permission to create and update equipment.
    /// </summary>
    public const string EquipmentsWrite = "facility.equipments.write";

    /// <summary>
    /// Permission to delete or manage equipment lifecycle (decommission).
    /// </summary>
    public const string EquipmentsManage = "facility.equipments.manage";

    /// <summary>
    /// Permission to read loan records.
    /// </summary>
    public const string LoansRead = "facility.loans.read";

    /// <summary>
    /// Permission to create and update loans.
    /// </summary>
    public const string LoansWrite = "facility.loans.write";

    /// <summary>
    /// Permission to approve loan requests.
    /// </summary>
    public const string LoansApprove = "facility.loans.approve";

    /// <summary>
    /// Permission to read maintenance logs.
    /// </summary>
    public const string MaintenanceRead = "facility.maintenance.read";

    /// <summary>
    /// Permission to create and update maintenance logs.
    /// </summary>
    public const string MaintenanceWrite = "facility.maintenance.write";

    /// <summary>
    /// Permission to read equipment attachments.
    /// </summary>
    public const string AttachmentsRead = "facility.attachments.read";

    /// <summary>
    /// Permission to manage equipment attachments.
    /// </summary>
    public const string AttachmentsWrite = "facility.attachments.write";

    /// <summary>
    /// All permissions with their descriptions.
    /// </summary>
    public static readonly Dictionary<string, string> AllWithDescriptions = new()
    {
        [EquipmentsRead] = "Read equipment information",
        [EquipmentsWrite] = "Create and update equipment",
        [EquipmentsManage] = "Delete or manage equipment lifecycle",
        [LoansRead] = "Read loan records",
        [LoansWrite] = "Create and update loans",
        [LoansApprove] = "Approve loan requests",
        [MaintenanceRead] = "Read maintenance logs",
        [MaintenanceWrite] = "Create and update maintenance logs",
        [AttachmentsRead] = "Read equipment attachments",
        [AttachmentsWrite] = "Manage equipment attachments"
    };
}
