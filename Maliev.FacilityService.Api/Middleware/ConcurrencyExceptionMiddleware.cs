using Microsoft.EntityFrameworkCore;

namespace Maliev.FacilityService.Api.Middleware;

/// <summary>
/// Middleware that catches <see cref="DbUpdateConcurrencyException"/> and returns HTTP 409 Conflict.
/// </summary>
public class ConcurrencyExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ConcurrencyExceptionMiddleware> _logger;

    /// <summary>Initializes a new instance of <see cref="ConcurrencyExceptionMiddleware"/>.</summary>
    /// <param name="next">The next middleware delegate in the pipeline.</param>
    /// <param name="logger">The logger instance.</param>
    public ConcurrencyExceptionMiddleware(RequestDelegate next, ILogger<ConcurrencyExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>Invokes the middleware, catching <see cref="DbUpdateConcurrencyException"/> and returning HTTP 409.</summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Concurrency conflict detected");
            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.10",
                title = "Conflict",
                status = 409,
                detail = "The record was modified by another request. Please reload and try again."
            });
        }
    }
}
