using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.Mapping;
using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Application.UseCases.Commands.RejectLoan;

/// <summary>
/// Handler for the <see cref="RejectLoanCommand"/>.
/// Rejects a pending loan request.
/// </summary>
public class RejectLoanCommandHandler
{
    private readonly ILoanRepository _loanRepository;
    private readonly IEquipmentRepository _equipmentRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="RejectLoanCommandHandler"/>.
    /// </summary>
    /// <param name="loanRepository">The loan repository.</param>
    /// <param name="equipmentRepository">The equipment repository.</param>
    public RejectLoanCommandHandler(
        ILoanRepository loanRepository,
        IEquipmentRepository equipmentRepository)
    {
        _loanRepository = loanRepository;
        _equipmentRepository = equipmentRepository;
    }

    /// <summary>
    /// Handles the rejection of a loan request.
    /// </summary>
    /// <param name="command">The reject loan command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated loan DTO with rejected status.</returns>
    public async Task<LoanDto> HandleAsync(
        RejectLoanCommand command,
        CancellationToken cancellationToken = default)
    {
        var loan = await _loanRepository.GetByIdAsync(command.LoanId, cancellationToken)
            ?? throw new KeyNotFoundException($"Loan '{command.LoanId}' not found.");

        if (loan.LoanStatus != LoanStatus.Pending)
            throw new InvalidOperationException(
                $"Loan '{command.LoanId}' cannot be rejected because it is in status '{loan.LoanStatus}'. Only Pending loans can be rejected.");

        var equipment = await _equipmentRepository.GetByIdAsync(loan.EquipmentId, cancellationToken);

        loan.LoanStatus = LoanStatus.Rejected;
        var updated = await _loanRepository.UpdateAsync(loan, cancellationToken);

        return updated.ToDto(equipment?.AssetCode ?? string.Empty);
    }
}
