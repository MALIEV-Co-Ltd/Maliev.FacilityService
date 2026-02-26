using System.Net.Http.Json;
using Maliev.FacilityService.Application.Interfaces;

namespace Maliev.FacilityService.Infrastructure.ExternalClients;

/// <summary>
/// HTTP client for querying job history from <c>Maliev.JobService</c>.
/// </summary>
public class JobServiceClient : IJobServiceClient
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of <see cref="JobServiceClient"/>.
    /// </summary>
    /// <param name="httpClient">The pre-configured <see cref="HttpClient"/> for the job service.</param>
    public JobServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<bool> HasHistoricalJobsAsync(Guid equipmentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"/job/v1/jobs/history?equipmentId={equipmentId}",
                cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }

            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<JobHistoryResponse>(
                cancellationToken: cancellationToken);

            return result?.HasHistory ?? false;
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    private sealed class JobHistoryResponse
    {
        /// <summary>Gets or sets a value indicating whether the equipment has historical job records.</summary>
        public bool HasHistory { get; set; }
    }
}
