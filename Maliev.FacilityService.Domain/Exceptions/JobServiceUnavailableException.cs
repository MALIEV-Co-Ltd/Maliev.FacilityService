namespace Maliev.FacilityService.Domain.Exceptions;

/// <summary>
/// Exception thrown when the downstream <c>Maliev.JobService</c> is unreachable or returns an unexpected error.
/// </summary>
public class JobServiceUnavailableException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="JobServiceUnavailableException"/>.
    /// </summary>
    public JobServiceUnavailableException()
        : base("The Job Service is temporarily unavailable. Please try again later.")
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="JobServiceUnavailableException"/> with a message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public JobServiceUnavailableException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="JobServiceUnavailableException"/> with a message and inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The underlying exception.</param>
    public JobServiceUnavailableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
