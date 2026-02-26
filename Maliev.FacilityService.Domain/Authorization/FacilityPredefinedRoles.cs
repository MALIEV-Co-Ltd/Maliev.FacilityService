namespace Maliev.FacilityService.Domain.Authorization;

/// <summary>
/// Defines predefined roles with their associated permission sets.
/// </summary>
public static class FacilityPredefinedRoles
{
    /// <summary>
    /// Administrator role with full access to all facility operations.
    /// </summary>
    public static readonly IReadOnlySet<string> Admin = new HashSet<string>
    {
        FacilityPermissions.EquipmentsRead,
        FacilityPermissions.EquipmentsWrite,
        FacilityPermissions.EquipmentsManage,
        FacilityPermissions.LoansRead,
        FacilityPermissions.LoansWrite,
        FacilityPermissions.LoansApprove,
        FacilityPermissions.MaintenanceRead,
        FacilityPermissions.MaintenanceWrite,
        FacilityPermissions.AttachmentsRead,
        FacilityPermissions.AttachmentsWrite
    };

    /// <summary>
    /// Manager role with access to equipment, loans, and maintenance operations.
    /// </summary>
    public static readonly IReadOnlySet<string> Manager = new HashSet<string>
    {
        FacilityPermissions.EquipmentsRead,
        FacilityPermissions.EquipmentsWrite,
        FacilityPermissions.LoansRead,
        FacilityPermissions.LoansWrite,
        FacilityPermissions.LoansApprove,
        FacilityPermissions.MaintenanceRead,
        FacilityPermissions.MaintenanceWrite,
        FacilityPermissions.AttachmentsRead,
        FacilityPermissions.AttachmentsWrite
    };

    /// <summary>
    /// Viewer role with read-only access to equipment, maintenance, and attachments.
    /// </summary>
    public static readonly IReadOnlySet<string> Viewer = new HashSet<string>
    {
        FacilityPermissions.EquipmentsRead,
        FacilityPermissions.MaintenanceRead,
        FacilityPermissions.AttachmentsRead
    };

    /// <summary>
    /// Technician role with read access to equipment and full maintenance and attachment operations.
    /// </summary>
    public static readonly IReadOnlySet<string> Technician = new HashSet<string>
    {
        FacilityPermissions.EquipmentsRead,
        FacilityPermissions.MaintenanceRead,
        FacilityPermissions.MaintenanceWrite,
        FacilityPermissions.AttachmentsRead,
        FacilityPermissions.AttachmentsWrite
    };
}
