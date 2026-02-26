using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.Mapping;
using Maliev.FacilityService.Domain.Entities;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Domain.Exceptions;

namespace Maliev.FacilityService.Application.UseCases.Commands.CreateLoan;

/// <summary>
/// Handler for the <see cref="CreateLoanCommand"/>.
/// Creates a new loan request, which may require approval for customer borrowers.
/// </summary>
public class CreateLoanCommandHandler
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly IEventPublisher _eventPublisher;

    /// <summary>
    /// Initializes a new instance of <see cref="CreateLoanCommandHandler"/>.
    /// </summary>
    /// <param name="equipmentRepository">The equipment repository.</param>
    /// <param name="loanRepository">The loan repository.</param>
    /// <param name="eventPublisher">The event publisher.</param>
    public CreateLoanCommandHandler(
        IEquipmentRepository equipmentRepository,
        ILoanRepository loanRepository,
        IEventPublisher eventPublisher)
    {
        _equipmentRepository = equipmentRepository;
        _loanRepository = loanRepository;
        _eventPublisher = eventPublisher;
    }

    /// <summary>
    /// Handles the creation of a new loan.
    /// </summary>
    /// <param name="command">The create loan command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created loan DTO.</returns>
    /// <exception cref="EquipmentNotFoundException">Thrown when the equipment is not found.</exception>
    public async Task<LoanDto> HandleAsync(
        CreateLoanCommand command,
        CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(command.EquipmentId, cancellationToken)
            ?? throw new EquipmentNotFoundException(command.EquipmentId);

        if (await _loanRepository.HasActiveLoanAsync(command.EquipmentId, cancellationToken))
            throw new LoanNotAllowedException(command.EquipmentId, "Equipment already has an active loan.");

        var loan = new EquipmentLoan
        {
            Id = Guid.NewGuid(),
            EquipmentId = command.EquipmentId,
            BorrowerId = command.BorrowerId,
            BorrowerType = command.BorrowerType,
            LoanStartDate = command.LoanStartDate,
            ExpectedReturnDate = command.ExpectedReturnDate,
            Purpose = command.Purpose
        };

        if (command.BorrowerType == LoanBorrowerType.Employee)
        {
            // Employee loan: activate immediately and change equipment status
            loan.LoanStatus = LoanStatus.Active;
            var previousStatus = equipment.Status;
            equipment.TransitionTo(EquipmentStatus.OnLoan);
            equipment.UpdatedAt = DateTime.UtcNow;
            await _equipmentRepository.UpdateAsync(equipment, cancellationToken);

            await _eventPublisher.PublishEquipmentStatusChangedAsync(
                equipment.Id,
                equipment.AssetCode,
                equipment.Name,
                equipment.Category.ToString(),
                previousStatus.ToString(),
                EquipmentStatus.OnLoan.ToString(),
                cancellationToken);
        }
        else
        {
            // Customer loan: pending approval, equipment status unchanged
            loan.LoanStatus = LoanStatus.Pending;
        }

        var saved = await _loanRepository.AddAsync(loan, cancellationToken);
        return saved.ToDto(equipment.AssetCode);
    }
}
