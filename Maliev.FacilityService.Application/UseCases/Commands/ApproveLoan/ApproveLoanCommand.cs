namespace Maliev.FacilityService.Application.UseCases.Commands.ApproveLoan;

/// <summary>
/// Command to approve a pending loan request.
/// Triggers a <c>LoanDocumentRequestedEvent</c> when the borrower is a customer.
/// </summary>
/// <param name="LoanId">ID of the loan to approve. Set by the controller from the route.</param>
/// <param name="ApprovedByEmployeeId">ID of the employee approving the loan.</param>
/// <param name="BorrowerDisplayName">Display name of the borrower, used in the loan PDF document.</param>
/// <param name="EquipmentRowVersion">xmin row version of the equipment for optimistic concurrency.</param>
/// <param name="LoanRowVersion">xmin row version of the loan for optimistic concurrency.</param>
public record ApproveLoanCommand(
    Guid LoanId,
    Guid ApprovedByEmployeeId,
    string BorrowerDisplayName,
    uint EquipmentRowVersion,
    uint LoanRowVersion);
