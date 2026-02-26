using Maliev.FacilityService.Domain.Exceptions;

namespace Maliev.FacilityService.Api.Middleware;

/// <summary>
/// Middleware that maps domain exceptions to appropriate HTTP responses.
/// </summary>
public class DomainExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<DomainExceptionMiddleware> _logger;

    /// <summary>Initializes a new instance of <see cref="DomainExceptionMiddleware"/>.</summary>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public DomainExceptionMiddleware(RequestDelegate next, ILogger<DomainExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>Invokes the middleware, mapping domain exceptions to HTTP responses.</summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (EquipmentNotFoundException ex)
        {
            _logger.LogInformation(ex, "Equipment not found");
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { title = "Not Found", status = 404, detail = ex.Message });
        }
        catch (InvalidStatusTransitionException ex)
        {
            _logger.LogInformation(ex, "Invalid status transition attempted");
            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { title = "Unprocessable Entity", status = 422, detail = ex.Message });
        }
        catch (AttachmentNotAllowedException ex)
        {
            _logger.LogInformation(ex, "Attachment not allowed for equipment category");
            context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { title = "Unprocessable Entity", status = 422, detail = ex.Message });
        }
        catch (LoanNotAllowedException ex)
        {
            _logger.LogInformation(ex, "Loan not allowed for equipment");
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { title = "Conflict", status = 409, detail = ex.Message });
        }
        catch (EquipmentHasJobHistoryException ex)
        {
            _logger.LogInformation(ex, "Equipment has job history, operation not allowed");
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new { title = "Conflict", status = 409, detail = ex.Message });
        }
        catch (JobServiceUnavailableException ex)
        {
            // JobService is unreachable — return 503 Service Unavailable.
            _logger.LogError(ex, "Job Service is unavailable");
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.6.4",
                title = "Service Unavailable",
                status = 503,
                detail = ex.Message
            });
        }
    }
}
