namespace Maliev.FacilityService.Domain.Exceptions;

/// <summary>
/// Exception thrown when attempting to hard delete equipment that has associated job history.
/// </summary>
public class EquipmentHasJobHistoryException : Exception
{
    /// <summary>
    /// Gets the ID of the equipment that has job history.
    /// </summary>
    public Guid EquipmentId { get; }

    /// <summary>
    /// Gets the number of job records associated with the equipment.
    /// </summary>
    public int JobCount { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentHasJobHistoryException"/> class.
    /// </summary>
    /// <param name="equipmentId">The ID of the equipment with job history.</param>
    /// <param name="jobCount">The number of job records.</param>
    public EquipmentHasJobHistoryException(Guid equipmentId, int jobCount)
        : base($"Equipment with ID '{equipmentId}' cannot be hard deleted because it has {jobCount} associated job record(s).")
    {
        EquipmentId = equipmentId;
        JobCount = jobCount;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentHasJobHistoryException"/> class with a custom message.
    /// </summary>
    /// <param name="message">The custom error message.</param>
    /// <param name="equipmentId">The ID of the equipment with job history.</param>
    /// <param name="jobCount">The number of job records.</param>
    public EquipmentHasJobHistoryException(string message, Guid equipmentId, int jobCount)
        : base(message)
    {
        EquipmentId = equipmentId;
        JobCount = jobCount;
    }
}
