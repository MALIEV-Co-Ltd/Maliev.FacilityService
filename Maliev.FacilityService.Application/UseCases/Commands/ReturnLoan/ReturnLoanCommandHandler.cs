using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.Mapping;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Domain.Exceptions;

namespace Maliev.FacilityService.Application.UseCases.Commands.ReturnLoan;

/// <summary>
/// Handler for the <see cref="ReturnLoanCommand"/>.
/// Records the return of equipment and updates loan and equipment status.
/// </summary>
public class ReturnLoanCommandHandler
{
    private readonly ILoanRepository _loanRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of <see cref="ReturnLoanCommandHandler"/>.
    /// </summary>
    /// <param name="loanRepository">The loan repository.</param>
    /// <param name="equipmentRepository">The equipment repository.</param>
    /// <param name="eventPublisher">The event publisher.</param>
    public ReturnLoanCommandHandler(
        ILoanRepository loanRepository,
        IEquipmentRepository equipmentRepository,
        IEventPublisher eventPublisher)
    {
        _loanRepository = loanRepository;
        _equipmentRepository = equipmentRepository;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Handles the return of loaned equipment.
    /// </summary>
    /// <param name="command">The return loan command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated loan DTO with returned status.</returns>
    /// <exception cref="EquipmentNotFoundException">Thrown when the associated equipment is not found.</exception>
    public async Task<LoanDto> HandleAsync(
        ReturnLoanCommand command,
        CancellationToken cancellationToken = default)
    {
        var loan = await _loanRepository.GetByIdAsync(command.LoanId, cancellationToken)
            ?? throw new KeyNotFoundException($"Loan '{command.LoanId}' not found.");

        var equipment = await _equipmentRepository.GetByIdAsync(loan.EquipmentId, cancellationToken)
            ?? throw new EquipmentNotFoundException(loan.EquipmentId);

        loan.LoanStatus = LoanStatus.Returned;
        loan.ActualReturnDate = command.ActualReturnDate;

        var previousStatus = equipment.Status;
        equipment.TransitionTo(EquipmentStatus.Active);
        equipment.UpdatedAt = DateTime.UtcNow;

        await _equipmentRepository.UpdateAsync(equipment, command.RowVersion, cancellationToken);
        var updatedLoan = await _loanRepository.UpdateAsync(loan, cancellationToken);

        await _eventPublisher.PublishEquipmentStatusChangedAsync(
            equipment.Id,
            equipment.AssetCode,
            equipment.Name,
            equipment.Category.ToString(),
            previousStatus.ToString(),
            EquipmentStatus.Active.ToString(),
            cancellationToken);

        return updatedLoan.ToDto(equipment.AssetCode);
    }
}
