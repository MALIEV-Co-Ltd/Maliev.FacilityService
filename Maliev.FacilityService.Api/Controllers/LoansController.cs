using Asp.Versioning;
using Maliev.Aspire.ServiceDefaults.Authorization;
using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.UseCases.Commands.ApproveLoan;
using Maliev.FacilityService.Application.UseCases.Commands.CreateLoan;
using Maliev.FacilityService.Application.UseCases.Commands.RejectLoan;
using Maliev.FacilityService.Application.UseCases.Commands.ReturnLoan;
using Maliev.FacilityService.Application.UseCases.Queries.GetEquipmentLoans;
using Maliev.FacilityService.Domain.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Maliev.FacilityService.Api.Controllers;

/// <summary>
/// Manages equipment loan requests including employee and customer lending with approval workflow.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("facility/v{version:apiVersion}")]
public class LoansController : FacilityControllerBase
{
    private readonly CreateLoanCommandHandler _createHandler;
    private readonly ApproveLoanCommandHandler _approveHandler;
    private readonly RejectLoanCommandHandler _rejectHandler;
    private readonly ReturnLoanCommandHandler _returnHandler;
    private readonly GetEquipmentLoansQueryHandler _getLoansHandler;

    /// <summary>
    /// Initializes a new instance of <see cref="LoansController"/>.
    /// </summary>
    /// <param name="createHandler">Handler for creating loan requests.</param>
    /// <param name="approveHandler">Handler for approving loan requests.</param>
    /// <param name="rejectHandler">Handler for rejecting loan requests.</param>
    /// <param name="returnHandler">Handler for recording equipment returns.</param>
    /// <param name="getLoansHandler">Handler for retrieving equipment loan history.</param>
    public LoansController(
        CreateLoanCommandHandler createHandler,
        ApproveLoanCommandHandler approveHandler,
        RejectLoanCommandHandler rejectHandler,
        ReturnLoanCommandHandler returnHandler,
        GetEquipmentLoansQueryHandler getLoansHandler)
    {
        _createHandler = createHandler;
        _approveHandler = approveHandler;
        _rejectHandler = rejectHandler;
        _returnHandler = returnHandler;
        _getLoansHandler = getLoansHandler;
    }

    /// <summary>
    /// Creates a new equipment loan request. Customer loans enter a Pending state awaiting approval.
    /// Employee loans may be directly activated.
    /// </summary>
    /// <param name="command">The loan request details including borrower type and dates.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created loan DTO.</returns>
    [HttpPost("loans")]
    [RequirePermission(FacilityPermissions.LoansWrite)]
    public async Task<ActionResult<LoanDto>> CreateLoan(
        [FromBody] CreateLoanCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = await _createHandler.HandleAsync(command, cancellationToken);
        return Created($"facility/v{ApiVersion}/equipments/{result.EquipmentId}/loans", result);
    }

    /// <summary>
    /// Approves a pending loan request. For customer loans, triggers a LoanDocumentRequestedEvent
    /// to generate the loan agreement PDF.
    /// </summary>
    /// <param name="id">The unique identifier of the loan to approve.</param>
    /// <param name="command">The approval details including row version for concurrency control.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated loan DTO with approved status.</returns>
    [HttpPatch("loans/{id:guid}/approve")]
    [RequirePermission(FacilityPermissions.LoansApprove)]
    public async Task<ActionResult<LoanDto>> ApproveLoan(
        [FromRoute] Guid id,
        [FromBody] ApproveLoanCommand command,
        CancellationToken cancellationToken = default)
    {
        var cmdWithId = command with { LoanId = id };
        var result = await _approveHandler.HandleAsync(cmdWithId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Rejects a pending loan request.
    /// </summary>
    /// <param name="id">The unique identifier of the loan to reject.</param>
    /// <param name="command">The rejection details including optional reason and row version.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated loan DTO with rejected status.</returns>
    [HttpPatch("loans/{id:guid}/reject")]
    [RequirePermission(FacilityPermissions.LoansApprove)]
    public async Task<ActionResult<LoanDto>> RejectLoan(
        [FromRoute] Guid id,
        [FromBody] RejectLoanCommand command,
        CancellationToken cancellationToken = default)
    {
        var cmdWithId = command with { LoanId = id };
        var result = await _rejectHandler.HandleAsync(cmdWithId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Records the return of equipment from an active loan.
    /// Updates equipment status back to Active.
    /// </summary>
    /// <param name="id">The unique identifier of the loan to mark as returned.</param>
    /// <param name="command">The return details including actual return date and row version.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated loan DTO with returned status.</returns>
    [HttpPatch("loans/{id:guid}/return")]
    [RequirePermission(FacilityPermissions.LoansWrite)]
    public async Task<ActionResult<LoanDto>> ReturnLoan(
        [FromRoute] Guid id,
        [FromBody] ReturnLoanCommand command,
        CancellationToken cancellationToken = default)
    {
        var cmdWithId = command with { LoanId = id };
        var result = await _returnHandler.HandleAsync(cmdWithId, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves the loan history for a specific piece of equipment, ordered by start date descending.
    /// </summary>
    /// <param name="equipmentId">The unique identifier of the equipment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of loan DTOs for the equipment.</returns>
    [HttpGet("equipments/{equipmentId:guid}/loans")]
    [RequirePermission(FacilityPermissions.LoansRead)]
    public async Task<ActionResult<IReadOnlyList<LoanDto>>> GetEquipmentLoans(
        [FromRoute] Guid equipmentId,
        CancellationToken cancellationToken = default)
    {
        var result = await _getLoansHandler.HandleAsync(
            new GetEquipmentLoansQuery(equipmentId),
            cancellationToken);
        return Ok(result);
    }
}
