using System.Net;
using System.Net.Http.Json;
using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Domain.Enums;
using Maliev.FacilityService.Tests.Infrastructure;
using Xunit;

namespace Maliev.FacilityService.Tests.Integration.Controllers;

[Collection("FacilityApiCollection")]
public class LoansControllerTests
{
    private readonly FacilityServiceTestFactory _factory;

    public LoansControllerTests(FacilityServiceTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetEquipmentLoans_ReturnsEmptyList_WhenNoLoans()
    {
        await _factory.CleanDatabaseAsync();
        var client = _factory.CreateAuthenticatedClient(
            "test-user",
            new[] { "roles.facility.technician" },
            new[] { "facility.equipments.read", "facility.loans.read" });

        var equipmentId = Guid.NewGuid();
        var response = await client.GetAsync($"/facility/v1/equipments/{equipmentId}/loans");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var loans = await response.Content.ReadFromJsonAsync<List<LoanDto>>();
        Assert.NotNull(loans);
        Assert.Empty(loans);
    }

    [Fact]
    public async Task CreateLoan_ReturnsBadRequest_WhenEquipmentNotFound()
    {
        await _factory.CleanDatabaseAsync();
        var client = _factory.CreateAuthenticatedClient(
            "test-user",
            new[] { "roles.facility.technician" },
            new[] { "facility.loans.write" });

        var loanCommand = new
        {
            equipmentId = Guid.NewGuid(),
            borrowerType = "Employee",
            borrowerId = Guid.NewGuid(),
            borrowerName = "Test User",
            scheduledStartDate = "2026-03-01",
            scheduledEndDate = "2026-03-15"
        };

        var response = await client.PostAsJsonAsync("/facility/v1/loans", loanCommand);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
