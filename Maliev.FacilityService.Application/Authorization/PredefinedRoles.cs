namespace Maliev.FacilityService.Application.Authorization;

/// <summary>
/// Provides access to predefined roles for the Facility Service.
/// </summary>
public static class FacilityPredefinedRoles
{
    public const string Admin = "roles.facility.admin";
    public const string Operator = "roles.facility.operator";
    public const string Viewer = "roles.facility.viewer";

    public static readonly IReadOnlyList<(string RoleId, string Description, string[] Permissions)> All = new List<(string, string, string[])>
    {
        (
            Admin,
            "Facility Administrator with full access",
            new[]
            {
                FacilityPermissions.EquipmentRead,
                FacilityPermissions.EquipmentWrite,
                FacilityPermissions.EquipmentManage,
                FacilityPermissions.LoanRead,
                FacilityPermissions.LoanWrite,
                FacilityPermissions.LoanApprove,
                FacilityPermissions.MaintenanceRead,
                FacilityPermissions.MaintenanceWrite,
                FacilityPermissions.AttachmentRead,
                FacilityPermissions.AttachmentWrite,
            }
        ),
        (
            Operator,
            "Facility Operator with equipment and maintenance access",
            new[]
            {
                FacilityPermissions.EquipmentRead,
                FacilityPermissions.EquipmentWrite,
                FacilityPermissions.LoanRead,
                FacilityPermissions.LoanWrite,
                FacilityPermissions.MaintenanceRead,
                FacilityPermissions.MaintenanceWrite,
                FacilityPermissions.AttachmentRead,
                FacilityPermissions.AttachmentWrite,
            }
        ),
        (
            Viewer,
            "Facility Viewer with read-only access",
            new[]
            {
                FacilityPermissions.EquipmentRead,
                FacilityPermissions.LoanRead,
                FacilityPermissions.MaintenanceRead,
                FacilityPermissions.AttachmentRead,
            }
        ),
    };
}
