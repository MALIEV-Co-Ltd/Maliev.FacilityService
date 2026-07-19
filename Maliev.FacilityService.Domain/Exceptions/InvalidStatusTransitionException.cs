namespace Maliev.FacilityService.Domain.Exceptions;

/// <summary>
/// Exception thrown when an invalid status transition is attempted on equipment.
/// </summary>
public class InvalidStatusTransitionException : Exception
{
    /// <summary>
    /// Gets the current status of the equipment.
    /// </summary>
    public string CurrentStatus { get; }

    /// <summary>
    /// Gets the attempted new status for the equipment.
    /// </summary>
    public string TargetStatus { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidStatusTransitionException"/> class.
    /// </summary>
    /// <param name="currentStatus">The current status of the equipment.</param>
    /// <param name="targetStatus">The attempted new status.</param>
    public InvalidStatusTransitionException(string currentStatus, string targetStatus)
        : base($"Invalid status transition from '{currentStatus}' to '{targetStatus}'.")
    {
        CurrentStatus = currentStatus;
        TargetStatus = targetStatus;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidStatusTransitionException"/> class with a custom message.
    /// </summary>
    /// <param name="message">The custom error message.</param>
    /// <param name="currentStatus">The current status of the equipment.</param>
    /// <param name="targetStatus">The attempted new status.</param>
    public InvalidStatusTransitionException(string message, string currentStatus, string targetStatus)
        : base(message)
    {
        CurrentStatus = currentStatus;
        TargetStatus = targetStatus;
    }
}
