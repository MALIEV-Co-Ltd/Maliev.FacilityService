using System.Net;
using System.Net.Http.Json;
using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Tests.Infrastructure;
using Xunit;

namespace Maliev.FacilityService.Tests.Integration.Controllers;

[Collection("FacilityApiCollection")]
public class EquipmentsControllerTests
{
    private readonly FacilityServiceTestFactory _factory;

    public EquipmentsControllerTests(FacilityServiceTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ListEquipments_ReturnsEmptyList_WhenNoEquipment()
    {
        await _factory.CleanDatabaseAsync();
        var client = _factory.CreateAuthenticatedClient(
            "test-user",
            new[] { "roles.facility.technician" },
            new[] { "facility.equipments.read" });

        var response = await client.GetAsync("/facility/v1/equipments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<EquipmentSummaryDto>>();
        Assert.NotNull(result);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task GetEquipmentById_ReturnsNotFound_WhenNotExists()
    {
        await _factory.CleanDatabaseAsync();
        var client = _factory.CreateAuthenticatedClient(
            "test-user",
            new[] { "roles.facility.technician" },
            new[] { "facility.equipments.read" });

        var response = await client.GetAsync($"/facility/v1/equipments/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetActiveEquipments_ReturnsEmptyList_WhenNoActiveEquipment()
    {
        await _factory.CleanDatabaseAsync();
        var client = _factory.CreateAuthenticatedClient(
            "test-user",
            new[] { "roles.facility.technician" },
            new[] { "facility.equipments.read", "facility.equipments.write" });

        var activeResponse = await client.GetAsync("/facility/v1/equipments/active");
        Assert.Equal(HttpStatusCode.OK, activeResponse.StatusCode);
        var result = await activeResponse.Content.ReadFromJsonAsync<List<ActiveEquipmentDto>>();
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ListEquipments_FiltersByStatus()
    {
        await _factory.CleanDatabaseAsync();
        var client = _factory.CreateAuthenticatedClient(
            "test-user",
            new[] { "roles.facility.technician" },
            new[] { "facility.equipments.read", "facility.equipments.write" });

        var activeResponse = await client.GetAsync("/facility/v1/equipments?status=Active");
        Assert.Equal(HttpStatusCode.OK, activeResponse.StatusCode);
        var activeResult = await activeResponse.Content.ReadFromJsonAsync<PagedResult<EquipmentSummaryDto>>();
        Assert.NotNull(activeResult);
    }

    [Fact]
    public async Task ListEquipments_FiltersByCategory()
    {
        await _factory.CleanDatabaseAsync();
        var client = _factory.CreateAuthenticatedClient(
            "test-user",
            new[] { "roles.facility.technician" },
            new[] { "facility.equipments.read", "facility.equipments.write" });

        var fdmResponse = await client.GetAsync("/facility/v1/equipments?category=FdmPrinter");
        Assert.Equal(HttpStatusCode.OK, fdmResponse.StatusCode);
        var fdmResult = await fdmResponse.Content.ReadFromJsonAsync<PagedResult<EquipmentSummaryDto>>();
        Assert.NotNull(fdmResult);
    }
}
