namespace Maliev.FacilityService.Domain.Enums;

/// <summary>
/// Represents the status of a loan request.
/// </summary>
public enum LoanStatus
{
    Pending,
    Approved,
    Rejected,
    Active,
    Returned,
    Overdue
}
