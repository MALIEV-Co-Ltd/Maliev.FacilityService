using Microsoft.AspNetCore.Mvc;

namespace Maliev.FacilityService.Api.Controllers;

/// <summary>
/// Base controller for all Facility Service endpoints.
/// Provides shared helpers such as API version resolution.
/// </summary>
public abstract class FacilityControllerBase : ControllerBase
{
    /// <summary>
    /// Returns the currently requested API version string (e.g. "1").
    /// Falls back to the configured default version so the fallback is never fragile.
    /// </summary>
    protected string ApiVersion =>
        HttpContext.GetRequestedApiVersion()?.MajorVersion?.ToString()
        ?? HttpContext.GetRequestedApiVersion()?.ToString()
        ?? "1";
}
