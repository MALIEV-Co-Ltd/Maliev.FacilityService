namespace Maliev.FacilityService.Application.Interfaces;

/// <summary>
/// Client interface for interacting with the external JobService.
/// Used to check job history for equipment before certain operations.
/// </summary>
public interface IJobServiceClient
{
    /// <summary>
    /// Checks if an equipment has any historical jobs in the JobService.
    /// </summary>
    /// <param name="equipmentId">The unique identifier of the equipment.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the equipment has historical jobs, otherwise false.</returns>
    /// <exception cref="HttpRequestException">Thrown when the JobService is unreachable.</exception>
    Task<bool> HasHistoricalJobsAsync(Guid equipmentId, CancellationToken cancellationToken = default);
}
