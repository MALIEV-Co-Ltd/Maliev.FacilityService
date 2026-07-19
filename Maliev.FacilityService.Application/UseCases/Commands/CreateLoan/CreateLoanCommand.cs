using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Application.UseCases.Commands.CreateLoan;

/// <summary>
/// Command to create a new equipment loan request.
/// </summary>
/// <param name="EquipmentId">ID of the equipment to loan.</param>
/// <param name="BorrowerId">ID of the borrower (employee or customer).</param>
/// <param name="BorrowerType">Type of borrower.</param>
/// <param name="LoanStartDate">Start date of the loan.</param>
/// <param name="ExpectedReturnDate">Expected return date.</param>
/// <param name="Purpose">Purpose of the loan.</param>
public record CreateLoanCommand(
    Guid EquipmentId,
    Guid BorrowerId,
    LoanBorrowerType BorrowerType,
    DateOnly LoanStartDate,
    DateOnly ExpectedReturnDate,
    string Purpose);
