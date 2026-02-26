namespace Maliev.FacilityService.Application.UseCases.Commands.RejectLoan;

/// <summary>
/// Command to reject a pending loan request.
/// </summary>
/// <param name="LoanId">ID of the loan to reject. Set by the controller from the route.</param>
/// <param name="Reason">Reason for rejection.</param>
/// <param name="RowVersion">xmin row version for optimistic concurrency.</param>
public record RejectLoanCommand(Guid LoanId, string? Reason, uint RowVersion);
