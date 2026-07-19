using System.Net;
using System.Net.Http.Json;
using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Tests.Infrastructure;
using Xunit;

namespace Maliev.FacilityService.Tests.Integration.Controllers;

[Collection("FacilityApiCollection")]
public class AttachmentsControllerTests
{
    private readonly FacilityServiceTestFactory _factory;

    public AttachmentsControllerTests(FacilityServiceTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAttachments_ReturnsEmptyList_WhenNoAttachments()
    {
        await _factory.CleanDatabaseAsync();
        var client = _factory.CreateAuthenticatedClient(
            "test-user",
            new[] { "roles.facility.technician" },
            new[] { "facility.equipments.read", "facility.attachments.read" });

        var equipmentId = Guid.NewGuid();
        var response = await client.GetAsync($"/facility/v1/equipments/{equipmentId}/attachments");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var attachments = await response.Content.ReadFromJsonAsync<List<AttachmentDto>>();
        Assert.NotNull(attachments);
        Assert.Empty(attachments);
    }

    [Fact]
    public async Task AddAttachment_ReturnsNotFound_WhenEquipmentNotFound()
    {
        await _factory.CleanDatabaseAsync();
        var client = _factory.CreateAuthenticatedClient(
            "test-user",
            new[] { "roles.facility.technician" },
            new[] { "facility.attachments.write" });

        var equipmentId = Guid.NewGuid();
        var attachmentCommand = new
        {
            name = "Test Attachment",
            attachmentType = "Tool",
            serialNumber = (string?)null,
            conditionNotes = (string?)null
        };

        var response = await client.PostAsJsonAsync($"/facility/v1/equipments/{equipmentId}/attachments", attachmentCommand);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAttachment_ReturnsNotFound_WhenNotFound()
    {
        await _factory.CleanDatabaseAsync();
        var client = _factory.CreateAuthenticatedClient(
            "test-user",
            new[] { "roles.facility.technician" },
            new[] { "facility.attachments.write" });

        var equipmentId = Guid.NewGuid();
        var attachmentId = Guid.NewGuid();
        var updateCommand = new { name = "Updated Name" };

        var response = await client.PutAsJsonAsync(
            $"/facility/v1/equipments/{equipmentId}/attachments/{attachmentId}",
            updateCommand);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
