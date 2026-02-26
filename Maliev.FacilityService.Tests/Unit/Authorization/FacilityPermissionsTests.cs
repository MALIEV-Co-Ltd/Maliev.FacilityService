using Maliev.FacilityService.Domain.Authorization;
using Xunit;

namespace Maliev.FacilityService.Tests.Unit.Authorization;

/// <summary>
/// Tests that verify all permissions in FacilityPermissions are assigned to at least one role.
/// </summary>
public class FacilityPermissionsTests
{
    private static readonly IReadOnlyList<IReadOnlySet<string>> AllRoles =
    [
        FacilityPredefinedRoles.Admin,
        FacilityPredefinedRoles.Manager,
        FacilityPredefinedRoles.Technician,
        FacilityPredefinedRoles.Viewer
    ];

    private static readonly IReadOnlyList<string> RoleNames =
    [
        "Admin",
        "Manager",
        "Technician",
        "Viewer"
    ];

    /// <summary>
    /// Verifies that every permission in FacilityPermissions.AllWithDescriptions is assigned to at least one role.
    /// </summary>
    [Fact]
    public void AllPermissions_MustBeAssignedToAtLeastOneRole()
    {
        var unassignedPermissions = new List<string>();

        foreach (var permissionKey in FacilityPermissions.AllWithDescriptions.Keys)
        {
            var isAssigned = AllRoles.Any(role => role.Contains(permissionKey));

            if (!isAssigned)
            {
                unassignedPermissions.Add(permissionKey);
            }
        }

        Assert.Empty(unassignedPermissions);
    }

    /// <summary>
    /// Verifies the total count of permissions matches the expected count.
    /// </summary>
    [Fact]
    public void PermissionCount_ShouldMatchExpected()
    {
        const int expectedPermissionCount = 10;
        Assert.Equal(expectedPermissionCount, FacilityPermissions.AllWithDescriptions.Count);
    }
}
