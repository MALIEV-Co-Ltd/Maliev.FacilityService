using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Application.DTOs;

/// <summary>
/// DTO representing an equipment loan record.
/// </summary>
public record LoanDto
{
    /// <summary>
    /// Unique identifier of the loan record.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// ID of the loaned equipment.
    /// </summary>
    public Guid EquipmentId { get; init; }

    /// <summary>
    /// Asset code of the loaned equipment.
    /// </summary>
    public string AssetCode { get; init; } = string.Empty;

    /// <summary>
    /// ID of the borrower (employee or customer).
    /// </summary>
    public Guid BorrowerId { get; init; }

    /// <summary>
    /// Type of borrower.
    /// </summary>
    public LoanBorrowerType BorrowerType { get; init; }

    /// <summary>
    /// Current status of the loan.
    /// </summary>
    public LoanStatus LoanStatus { get; init; }

    /// <summary>
    /// Start date of the loan.
    /// </summary>
    public DateOnly LoanStartDate { get; init; }

    /// <summary>
    /// Expected return date of the loan.
    /// </summary>
    public DateOnly ExpectedReturnDate { get; init; }

    /// <summary>
    /// Actual return date of the loan.
    /// </summary>
    public DateOnly? ActualReturnDate { get; init; }

    /// <summary>
    /// Purpose of the loan.
    /// </summary>
    public string Purpose { get; init; } = string.Empty;
}
