using System.Net;
using System.Net.Http.Json;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Domain.Exceptions;
using Maliev.FacilityService.Infrastructure.ExternalClients;

namespace Maliev.FacilityService.Tests.Unit.Infrastructure;

public class JobServiceClientTests
{
    [Fact]
    public async Task HasHistoricalJobsAsync_WithHistory_ReturnsTrue()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { HasHistory = true })
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test") };
        var client = new JobServiceClient(httpClient);

        var result = await client.HasHistoricalJobsAsync(Guid.NewGuid());

        Assert.True(result);
    }

    [Fact]
    public async Task HasHistoricalJobsAsync_WithoutHistory_ReturnsFalse()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { HasHistory = false })
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test") };
        var client = new JobServiceClient(httpClient);

        var result = await client.HasHistoricalJobsAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task HasHistoricalJobsAsync_NotFound_ReturnsFalse()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.NotFound));
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test") };
        var client = new JobServiceClient(httpClient);

        var result = await client.HasHistoricalJobsAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task HasHistoricalJobsAsync_HttpError_ThrowsJobServiceUnavailableException()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test") };
        var client = new JobServiceClient(httpClient);

        await Assert.ThrowsAsync<JobServiceUnavailableException>(
            () => client.HasHistoricalJobsAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task HasHistoricalJobsAsync_NullResponse_ReturnsFalse()
    {
        var handler = new MockHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { })
        });
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://test") };
        var client = new JobServiceClient(httpClient);

        var result = await client.HasHistoricalJobsAsync(Guid.NewGuid());

        Assert.False(result);
    }

    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public MockHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_response);
        }
    }
}
