namespace Maliev.FacilityService.Domain.Exceptions;

/// <summary>
/// Exception thrown when equipment is not found in the system.
/// </summary>
public class EquipmentNotFoundException : Exception
{
    /// <summary>
    /// Gets the ID of the equipment that was not found.
    /// </summary>
    public Guid EquipmentId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentNotFoundException"/> class.
    /// </summary>
    /// <param name="equipmentId">The ID of the equipment that was not found.</param>
    public EquipmentNotFoundException(Guid equipmentId)
        : base($"Equipment with ID '{equipmentId}' was not found.")
    {
        EquipmentId = equipmentId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentNotFoundException"/> class with a custom message.
    /// </summary>
    /// <param name="message">The custom error message.</param>
    /// <param name="equipmentId">The ID of the equipment that was not found.</param>
    public EquipmentNotFoundException(string message, Guid equipmentId)
        : base(message)
    {
        EquipmentId = equipmentId;
    }
}
