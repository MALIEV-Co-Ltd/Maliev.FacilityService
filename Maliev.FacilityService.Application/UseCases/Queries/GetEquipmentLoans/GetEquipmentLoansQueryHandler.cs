using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.Mapping;
using Maliev.FacilityService.Domain.Exceptions;

namespace Maliev.FacilityService.Application.UseCases.Queries.GetEquipmentLoans;

/// <summary>
/// Handler for the <see cref="GetEquipmentLoansQuery"/>.
/// Returns all loan records for the specified equipment, ordered by start date descending.
/// </summary>
public class GetEquipmentLoansQueryHandler
{
    private readonly ILoanRepository _loanRepository;
    private readonly IEquipmentRepository _equipmentRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="GetEquipmentLoansQueryHandler"/>.
    /// </summary>
    /// <param name="loanRepository">The loan repository.</param>
    /// <param name="equipmentRepository">The equipment repository.</param>
    public GetEquipmentLoansQueryHandler(
        ILoanRepository loanRepository,
        IEquipmentRepository equipmentRepository)
    {
        _loanRepository = loanRepository;
        _equipmentRepository = equipmentRepository;
    }

    /// <summary>
    /// Handles retrieval of equipment loan history.
    /// </summary>
    /// <param name="query">The get loans query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of loan DTOs for the equipment.</returns>
    /// <exception cref="EquipmentNotFoundException">Thrown when the equipment is not found.</exception>
    public async Task<IReadOnlyList<LoanDto>> HandleAsync(
        GetEquipmentLoansQuery query,
        CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(query.EquipmentId, cancellationToken)
            ?? throw new EquipmentNotFoundException(query.EquipmentId);

        var loans = await _loanRepository.GetByEquipmentIdAsync(query.EquipmentId, cancellationToken);
        return loans.Select(l => l.ToDto(equipment.AssetCode)).ToList();
    }
}
