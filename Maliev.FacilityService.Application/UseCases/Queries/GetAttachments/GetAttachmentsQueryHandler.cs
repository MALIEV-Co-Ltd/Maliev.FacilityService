using Maliev.FacilityService.Application.DTOs;
using Maliev.FacilityService.Application.Interfaces;
using Maliev.FacilityService.Application.Mapping;

namespace Maliev.FacilityService.Application.UseCases.Queries.GetAttachments;

/// <summary>
/// Handler for the <see cref="GetAttachmentsQuery"/>.
/// Returns all attachments for the specified equipment.
/// </summary>
public class GetAttachmentsQueryHandler
{
    private readonly IAttachmentRepository _attachmentRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="GetAttachmentsQueryHandler"/>.
    /// </summary>
    /// <param name="attachmentRepository">The attachment repository.</param>
    public GetAttachmentsQueryHandler(IAttachmentRepository attachmentRepository)
    {
        _attachmentRepository = attachmentRepository;
    }

    /// <summary>
    /// Handles retrieval of equipment attachments.
    /// </summary>
    /// <param name="query">The get attachments query.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of attachment DTOs for the equipment.</returns>
    public async Task<IReadOnlyList<AttachmentDto>> HandleAsync(
        GetAttachmentsQuery query,
        CancellationToken cancellationToken = default)
    {
        var attachments = await _attachmentRepository.GetByEquipmentIdAsync(query.EquipmentId, cancellationToken);
        return attachments.Select(a => a.ToDto()).ToList();
    }
}
