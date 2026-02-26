using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Domain.Exceptions;

namespace Maliev.FacilityService.Application.UseCases.Commands.DeleteEquipment;

/// <summary>
/// Handler for the <see cref="DeleteEquipmentCommand"/>.
/// Checks job history before hard-deleting equipment.
/// </summary>
public class DeleteEquipmentCommandHandler
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IJobServiceClient _jobServiceClient;
    private readonly ILoanRepository _loanRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="DeleteEquipmentCommandHandler"/>.
    /// </summary>
    /// <param name="equipmentRepository">The equipment repository.</param>
    /// <param name="jobServiceClient">The job service client.</param>
    /// <param name="loanRepository">The loan repository.</param>
    public DeleteEquipmentCommandHandler(
        IEquipmentRepository equipmentRepository,
        IJobServiceClient jobServiceClient,
        ILoanRepository loanRepository)
    {
        _equipmentRepository = equipmentRepository;
        _jobServiceClient = jobServiceClient;
        _loanRepository = loanRepository;
    }

    /// <summary>
    /// Handles the deletion of equipment.
    /// </summary>
    /// <param name="command">The delete equipment command.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <exception cref="EquipmentNotFoundException">Thrown when the equipment is not found.</exception>
    /// <exception cref="EquipmentHasJobHistoryException">Thrown when job history exists, blocking hard delete.</exception>
    /// <exception cref="LoanNotAllowedException">Thrown when equipment has an active loan.</exception>
    public async Task HandleAsync(
        DeleteEquipmentCommand command,
        CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(command.EquipmentId, cancellationToken)
            ?? throw new EquipmentNotFoundException(command.EquipmentId);

        // Check active loans
        var hasActiveLoan = await _loanRepository.HasActiveLoanAsync(command.EquipmentId, cancellationToken);
        if (hasActiveLoan)
            throw new LoanNotAllowedException(command.EquipmentId, "Equipment has an active loan and cannot be deleted.");

        // Check historical jobs — throws HttpRequestException if unreachable (503 sent by global middleware)
        var hasHistory = await _jobServiceClient.HasHistoricalJobsAsync(command.EquipmentId, cancellationToken);
        if (hasHistory)
            throw new EquipmentHasJobHistoryException(command.EquipmentId, 1);

        await _equipmentRepository.DeleteAsync(equipment.Id, cancellationToken);
    }
}
