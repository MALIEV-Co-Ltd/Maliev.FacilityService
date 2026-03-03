using Maliev.FacilityService.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Maliev.FacilityService.Tests.Unit.Middleware;

public class ConcurrencyExceptionMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<ILogger<ConcurrencyExceptionMiddleware>> _loggerMock;
    private readonly ConcurrencyExceptionMiddleware _middleware;

    public ConcurrencyExceptionMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _loggerMock = new Mock<ILogger<ConcurrencyExceptionMiddleware>>();
        _middleware = new ConcurrencyExceptionMiddleware(
            _nextMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task InvokeAsync_NoException_CallsNext()
    {
        var context = new DefaultHttpContext();
        _nextMock.Setup(n => n.Invoke(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        await _middleware.InvokeAsync(context);

        _nextMock.Verify(n => n.Invoke(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_DbUpdateConcurrencyException_Returns409()
    {
        var context = new DefaultHttpContext();
        var exception = new DbUpdateConcurrencyException("Concurrency conflict");
        _nextMock.Setup(n => n.Invoke(It.IsAny<HttpContext>())).Throws(exception);

        await _middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_DbUpdateConcurrencyException_SetsJsonContentType()
    {
        var context = new DefaultHttpContext();
        var exception = new DbUpdateConcurrencyException("Concurrency conflict");
        _nextMock.Setup(n => n.Invoke(It.IsAny<HttpContext>())).Throws(exception);

        await _middleware.InvokeAsync(context);

        Assert.StartsWith("application/json", context.Response.ContentType);
    }
}
