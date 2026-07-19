namespace Maliev.FacilityService.Application.Authorization;

/// <summary>
/// Defines the permissions for the Facility Service.
/// </summary>
public static class FacilityPermissions
{
    public const string EquipmentRead = "facility.equipments.read";
    public const string EquipmentWrite = "facility.equipments.write";
    public const string EquipmentManage = "facility.equipments.manage";

    public const string LoanRead = "facility.loans.read";
    public const string LoanWrite = "facility.loans.write";
    public const string LoanApprove = "facility.loans.approve";

    public const string MaintenanceRead = "facility.maintenance.read";
    public const string MaintenanceWrite = "facility.maintenance.write";

    public const string AttachmentRead = "facility.attachments.read";
    public const string AttachmentWrite = "facility.attachments.write";

    public static readonly IReadOnlyDictionary<string, string> AllWithDescriptions = new Dictionary<string, string>
    {
        { EquipmentRead, "Read equipment data" },
        { EquipmentWrite, "Write equipment data" },
        { EquipmentManage, "Manage equipment" },
        { LoanRead, "Read facility loans" },
        { LoanWrite, "Write facility loans" },
        { LoanApprove, "Approve facility loans" },
        { MaintenanceRead, "Read maintenance records" },
        { MaintenanceWrite, "Write maintenance records" },
        { AttachmentRead, "Read facility attachments" },
        { AttachmentWrite, "Write facility attachments" },
    };

    public static string[] All => AllWithDescriptions.Keys.ToArray();
}
