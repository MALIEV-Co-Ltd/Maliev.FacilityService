namespace Maliev.FacilityService.Domain.Exceptions;

/// <summary>
/// Exception thrown when a loan cannot be created for equipment.
/// </summary>
public class LoanNotAllowedException : Exception
{
    /// <summary>
    /// Gets the ID of the equipment.
    /// </summary>
    public Guid EquipmentId { get; }

    /// <summary>
    /// Gets the reason why the loan is not allowed.
    /// </summary>
    public string Reason { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoanNotAllowedException"/> class.
    /// </summary>
    /// <param name="equipmentId">The ID of the equipment.</param>
    /// <param name="reason">The reason why the loan is not allowed.</param>
    public LoanNotAllowedException(Guid equipmentId, string reason)
        : base($"Loan not allowed for equipment '{equipmentId}': {reason}")
    {
        EquipmentId = equipmentId;
        Reason = reason;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LoanNotAllowedException"/> class with a custom message.
    /// </summary>
    /// <param name="message">The custom error message.</param>
    /// <param name="equipmentId">The ID of the equipment.</param>
    /// <param name="reason">The reason why the loan is not allowed.</param>
    public LoanNotAllowedException(string message, Guid equipmentId, string reason)
        : base(message)
    {
        EquipmentId = equipmentId;
        Reason = reason;
    }
}
