using Maliev.Aspire.ServiceDefaults.IAM;
using Maliev.FacilityService.Domain.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Maliev.FacilityService.Api.Services;

/// <summary>
/// Registers Facility Service permissions and roles with the central IAM service on startup.
/// </summary>
public class FacilityIAMRegistrationService : IAMRegistrationService
{
    /// <summary>
    /// Initializes a new instance of <see cref="FacilityIAMRegistrationService"/>.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="logger">The logger instance.</param>
    public FacilityIAMRegistrationService(
        IConfiguration configuration,
        ILogger<FacilityIAMRegistrationService> logger)
        : base(configuration, logger, "facility")
    {
    }

    /// <inheritdoc />
    protected override IEnumerable<PermissionRegistration> GetPermissions()
    {
        return FacilityPermissions.AllWithDescriptions.Select(p => new PermissionRegistration
        {
            PermissionId = p.Key,
            Description = p.Value
        });
    }

    /// <inheritdoc />
    protected override IEnumerable<RoleRegistration> GetPredefinedRoles()
    {
        return
        [
            new RoleRegistration
            {
                RoleId = "facility.admin",
                Description = "Full access to all facility operations including equipment lifecycle, loans, maintenance, and attachments.",
                PermissionIds = FacilityPredefinedRoles.Admin.ToList()
            },
            new RoleRegistration
            {
                RoleId = "facility.manager",
                Description = "Manages equipment, approves loans, and records maintenance. Cannot delete or decommission equipment.",
                PermissionIds = FacilityPredefinedRoles.Manager.ToList()
            },
            new RoleRegistration
            {
                RoleId = "facility.technician",
                Description = "Records and reads maintenance logs and attachments. Read-only access to equipment.",
                PermissionIds = FacilityPredefinedRoles.Technician.ToList()
            },
            new RoleRegistration
            {
                RoleId = "facility.viewer",
                Description = "Read-only access to equipment, maintenance logs, and attachments.",
                PermissionIds = FacilityPredefinedRoles.Viewer.ToList()
            }
        ];
    }
}
