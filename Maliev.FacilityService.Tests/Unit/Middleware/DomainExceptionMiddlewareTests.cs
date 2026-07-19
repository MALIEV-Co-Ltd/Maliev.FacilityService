using Maliev.FacilityService.Api.Middleware;
using Maliev.FacilityService.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Maliev.FacilityService.Tests.Unit.Middleware;

public class DomainExceptionMiddlewareTests
{
    private readonly Mock<RequestDelegate> _nextMock;
    private readonly Mock<ILogger<DomainExceptionMiddleware>> _loggerMock;
    private readonly DomainExceptionMiddleware _middleware;

    public DomainExceptionMiddlewareTests()
    {
        _nextMock = new Mock<RequestDelegate>();
        _loggerMock = new Mock<ILogger<DomainExceptionMiddleware>>();
        _middleware = new DomainExceptionMiddleware(
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
    public async Task InvokeAsync_EquipmentNotFoundException_Returns404()
    {
        var context = new DefaultHttpContext();
        var exception = new EquipmentNotFoundException(Guid.NewGuid());
        _nextMock.Setup(n => n.Invoke(It.IsAny<HttpContext>())).Throws(exception);

        await _middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_InvalidStatusTransitionException_Returns422()
    {
        var context = new DefaultHttpContext();
        var exception = new InvalidStatusTransitionException("Active", "Decommissioned");
        _nextMock.Setup(n => n.Invoke(It.IsAny<HttpContext>())).Throws(exception);

        await _middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status422UnprocessableEntity, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_AttachmentNotAllowedException_Returns422()
    {
        var context = new DefaultHttpContext();
        var exception = new AttachmentNotAllowedException(Guid.NewGuid(), "OfficeEquipment");
        _nextMock.Setup(n => n.Invoke(It.IsAny<HttpContext>())).Throws(exception);

        await _middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status422UnprocessableEntity, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_LoanNotAllowedException_Returns409()
    {
        var context = new DefaultHttpContext();
        var exception = new LoanNotAllowedException(Guid.NewGuid(), "Equipment already on loan");
        _nextMock.Setup(n => n.Invoke(It.IsAny<HttpContext>())).Throws(exception);

        await _middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_EquipmentHasJobHistoryException_Returns409()
    {
        var context = new DefaultHttpContext();
        var exception = new EquipmentHasJobHistoryException(Guid.NewGuid(), 5);
        _nextMock.Setup(n => n.Invoke(It.IsAny<HttpContext>())).Throws(exception);

        await _middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status409Conflict, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_JobServiceUnavailableException_Returns503()
    {
        var context = new DefaultHttpContext();
        var exception = new JobServiceUnavailableException("Job service is unavailable");
        _nextMock.Setup(n => n.Invoke(It.IsAny<HttpContext>())).Throws(exception);

        await _middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status503ServiceUnavailable, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_EquipmentNotFoundException_SetsJsonContentType()
    {
        var context = new DefaultHttpContext();
        var exception = new EquipmentNotFoundException(Guid.NewGuid());
        _nextMock.Setup(n => n.Invoke(It.IsAny<HttpContext>())).Throws(exception);

        await _middleware.InvokeAsync(context);

        Assert.StartsWith("application/json", context.Response.ContentType);
    }

    [Fact]
    public async Task InvokeAsync_InvalidStatusTransitionException_SetsJsonContentType()
    {
        var context = new DefaultHttpContext();
        var exception = new InvalidStatusTransitionException("Active", "Lost");
        _nextMock.Setup(n => n.Invoke(It.IsAny<HttpContext>())).Throws(exception);

        await _middleware.InvokeAsync(context);

        Assert.StartsWith("application/json", context.Response.ContentType);
    }
}
