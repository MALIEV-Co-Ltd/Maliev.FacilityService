using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Domain.Entities;

/// <summary>
/// Represents a loan record for equipment lent to employees or customers.
/// </summary>
public class EquipmentLoan
{
    /// <summary>
    /// Unique identifier for the loan record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the loaned equipment.
    /// </summary>
    public Guid EquipmentId { get; set; }

    /// <summary>
    /// ID of the borrower (employee or customer).
    /// </summary>
    public Guid BorrowerId { get; set; }

    /// <summary>
    /// Type of borrower (Employee or Customer).
    /// </summary>
    public LoanBorrowerType BorrowerType { get; set; }

    /// <summary>
    /// Employee ID who approved the loan.
    /// </summary>
    public Guid? ApprovedByEmployeeId { get; set; }

    /// <summary>
    /// Start date of the loan.
    /// </summary>
    public DateOnly LoanStartDate { get; set; }

    /// <summary>
    /// Expected return date of the loan.
    /// </summary>
    public DateOnly ExpectedReturnDate { get; set; }

    /// <summary>
    /// Actual return date of the loan.
    /// </summary>
    public DateOnly? ActualReturnDate { get; set; }

    /// <summary>
    /// Purpose of the loan.
    /// </summary>
    public string Purpose { get; set; } = string.Empty;

    /// <summary>
    /// Notes on the condition of equipment upon return.
    /// </summary>
    public string? ReturnConditionNotes { get; set; }

    /// <summary>
    /// Current status of the loan.
    /// </summary>
    public LoanStatus LoanStatus { get; set; }
}
