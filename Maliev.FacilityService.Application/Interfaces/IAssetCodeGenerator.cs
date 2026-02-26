using Maliev.FacilityService.Domain.Enums;

namespace Maliev.FacilityService.Application.Interfaces;

/// <summary>
/// Service interface for generating unique asset codes for equipment.
/// </summary>
public interface IAssetCodeGenerator
{
    /// <summary>
    /// Asynchronously generates a unique asset code for the specified equipment category.
    /// </summary>
    /// <param name="category">The equipment category.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A unique asset code in the format MAL-{PREFIX}-{SEQ}.</returns>
    Task<string> GenerateAssetCodeAsync(
        EquipmentCategory category,
        CancellationToken cancellationToken = default);
}
