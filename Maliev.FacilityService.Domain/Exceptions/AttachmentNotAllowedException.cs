namespace Maliev.FacilityService.Domain.Exceptions;

/// <summary>
/// Exception thrown when an attachment is attempted to be added to non-CNC equipment.
/// </summary>
public class AttachmentNotAllowedException : Exception
{
    /// <summary>
    /// Gets the ID of the equipment.
    /// </summary>
    public Guid EquipmentId { get; }

    /// <summary>
    /// Gets the category of the equipment.
    /// </summary>
    public string EquipmentCategory { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AttachmentNotAllowedException"/> class.
    /// </summary>
    /// <param name="equipmentId">The ID of the equipment.</param>
    /// <param name="equipmentCategory">The category of the equipment.</param>
    public AttachmentNotAllowedException(Guid equipmentId, string equipmentCategory)
        : base($"Attachments are not allowed for equipment category '{equipmentCategory}'. Only CNC machines can have attachments.")
    {
        EquipmentId = equipmentId;
        EquipmentCategory = equipmentCategory;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AttachmentNotAllowedException"/> class with a custom message.
    /// </summary>
    /// <param name="message">The custom error message.</param>
    /// <param name="equipmentId">The ID of the equipment.</param>
    /// <param name="equipmentCategory">The category of the equipment.</param>
    public AttachmentNotAllowedException(string message, Guid equipmentId, string equipmentCategory)
        : base(message)
    {
        EquipmentId = equipmentId;
        EquipmentCategory = equipmentCategory;
    }
}
