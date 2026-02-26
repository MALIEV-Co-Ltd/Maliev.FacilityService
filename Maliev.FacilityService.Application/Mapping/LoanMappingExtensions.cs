using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Domain.Entities;

namespace Maliev.FacilityService.Application.Mapping;

/// <summary>
/// Manual mapping extension methods for converting loan domain entities to DTOs.
/// </summary>
public static class LoanMappingExtensions
{
    /// <summary>
    /// Maps an <see cref="EquipmentLoan"/> entity to a <see cref="LoanDto"/>.
    /// </summary>
    /// <param name="loan">The loan entity to map.</param>
    /// <param name="assetCode">The asset code of the associated equipment.</param>
    /// <returns>A <see cref="LoanDto"/> populated from the entity.</returns>
    public static LoanDto ToDto(this EquipmentLoan loan, string assetCode) =>
        new()
        {
            Id = loan.Id,
            EquipmentId = loan.EquipmentId,
            AssetCode = assetCode,
            BorrowerId = loan.BorrowerId,
            BorrowerType = loan.BorrowerType,
            LoanStatus = loan.LoanStatus,
            LoanStartDate = loan.LoanStartDate,
            ExpectedReturnDate = loan.ExpectedReturnDate,
            ActualReturnDate = loan.ActualReturnDate,
            Purpose = loan.Purpose
        };
}
