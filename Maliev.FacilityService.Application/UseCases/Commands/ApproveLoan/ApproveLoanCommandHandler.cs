using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.Mapping;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Domain.Exceptions;

namespace Maliev.FacilityService.Application.UseCases.Commands.ApproveLoan;

/// <summary>
/// Handler for the <see cref="ApproveLoanCommand"/>.
/// Approves a pending loan and publishes a loan document event for customer loans.
/// </summary>
public class ApproveLoanCommandHandler
{
    private readonly ILoanRepository _loanRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of <see cref="ApproveLoanCommandHandler"/>.
    /// </summary>
    /// <param name="loanRepository">The loan repository.</param>
    /// <param name="equipmentRepository">The equipment repository.</param>
    /// <param name="eventPublisher">The event publisher.</param>
    public ApproveLoanCommandHandler(
        ILoanRepository loanRepository,
        IEquipmentRepository equipmentRepository,
        IEventPublisher eventPublisher)
    {
        _loanRepository = loanRepository;
        _equipmentRepository = equipmentRepository;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Handles the approval of a loan request.
    /// </summary>
    /// <param name="command">The approve loan command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated loan DTO with approved status.</returns>
    /// <exception cref="EquipmentNotFoundException">Thrown when the associated equipment is not found.</exception>
    public async Task<LoanDto> HandleAsync(
        ApproveLoanCommand command,
        CancellationToken cancellationToken = default)
    {
        var loan = await _loanRepository.GetByIdAsync(command.LoanId, cancellationToken)
            ?? throw new KeyNotFoundException($"Loan '{command.LoanId}' not found.");

        var equipment = await _equipmentRepository.GetByIdAsync(loan.EquipmentId, cancellationToken)
            ?? throw new EquipmentNotFoundException(loan.EquipmentId);

        loan.LoanStatus = LoanStatus.Active;
        loan.ApprovedByEmployeeId = command.ApprovedByEmployeeId;

        var previousStatus = equipment.Status;
        equipment.TransitionTo(EquipmentStatus.OnLoan);
        equipment.UpdatedAt = DateTime.UtcNow;

        await _equipmentRepository.UpdateAsync(equipment, command.EquipmentRowVersion, cancellationToken);
        var updatedLoan = await _loanRepository.UpdateAsync(loan, command.LoanRowVersion, cancellationToken);

        // Publish equipment status changed event
        await _eventPublisher.PublishEquipmentStatusChangedAsync(
            equipment.Id,
            equipment.AssetCode,
            equipment.Name,
            equipment.Category.ToString(),
            previousStatus.ToString(),
            EquipmentStatus.OnLoan.ToString(),
            cancellationToken);

        // Publish loan document requested event for customer borrowers
        if (loan.BorrowerType == LoanBorrowerType.Customer)
        {
            await _eventPublisher.PublishLoanDocumentRequestedAsync(
                loan.Id,
                equipment.Id,
                equipment.AssetCode,
                equipment.Name,
                equipment.Brand,
                equipment.ModelName,
                equipment.ManufacturerSerialNumber,
                loan.BorrowerId,
                command.BorrowerDisplayName,
                loan.BorrowerType.ToString(),
                command.ApprovedByEmployeeId,
                loan.LoanStartDate,
                loan.ExpectedReturnDate,
                loan.Purpose,
                cancellationToken);
        }

        return updatedLoan.ToDto(equipment.AssetCode);
    }
}
