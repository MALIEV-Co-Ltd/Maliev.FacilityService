namespace Maliev.FacilityService.Application.UseCases.Commands.ReturnLoan;

/// <summary>
/// Command to record the return of equipment from a loan.
/// </summary>
/// <param name="LoanId">ID of the loan to mark as returned. Set by the controller from the route.</param>
/// <param name="ActualReturnDate">Actual date of return.</param>
/// <param name="RowVersion">xmin row version for optimistic concurrency.</param>
public record ReturnLoanCommand(Guid LoanId, DateOnly ActualReturnDate, uint RowVersion);
