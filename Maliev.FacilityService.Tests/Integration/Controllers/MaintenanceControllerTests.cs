using System.Net;
using System.Net.Http.Json;
using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Tests.Infrastructure;
using Xunit;

namespace Maliev.FacilityService.Tests.Integration.Controllers;

[Collection("FacilityApiCollection")]
public class MaintenanceControllerTests
{
    private readonly FacilityServiceTestFactory _factory;

    public MaintenanceControllerTests(FacilityServiceTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetMaintenanceLogs_ReturnsEmptyList_WhenNoLogs()
    {
        await _factory.CleanDatabaseAsync();
        var client = _factory.CreateAuthenticatedClient(
            "test-user",
            new[] { "roles.facility.technician" },
            new[] { "facility.equipments.read", "facility.maintenance.read" });

        var equipmentId = Guid.NewGuid();
        var response = await client.GetAsync($"/facility/v1/equipments/{equipmentId}/maintenance");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var logs = await response.Content.ReadFromJsonAsync<List<MaintenanceLogDto>>();
        Assert.NotNull(logs);
        Assert.Empty(logs);
    }

    [Fact]
    public async Task AddMaintenanceLog_ReturnsNotFound_WhenEquipmentNotFound()
    {
        await _factory.CleanDatabaseAsync();
        var client = _factory.CreateAuthenticatedClient(
            "test-user",
            new[] { "roles.facility.technician" },
            new[] { "facility.maintenance.write" });

        var equipmentId = Guid.NewGuid();
        var maintenanceCommand = new
        {
            maintenanceType = "Preventive",
            occurrenceDate = "2026-03-01",
            description = "Test maintenance"
        };

        var response = await client.PostAsJsonAsync($"/facility/v1/equipments/{equipmentId}/maintenance", maintenanceCommand);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
