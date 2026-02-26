namespace Maliev.FacilityService.Application.UseCases.Queries.GetAttachments;

/// <summary>
/// Query to retrieve all attachments for a specific equipment.
/// </summary>
/// <param name="EquipmentId">The unique identifier of the equipment.</param>
public record GetAttachmentsQuery(Guid EquipmentId);
