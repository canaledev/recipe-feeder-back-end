using System.Text.Json;
using Feedy.Api.Middleware;
using Xunit;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Feedy.Api.Tests.Middleware;

public class GlobalExceptionHandlerMiddlewareTests
{
    private readonly Mock<ILogger<GlobalExceptionHandlerMiddleware>> _logger = new();
    private readonly Mock<IWebHostEnvironment> _env = new();

    private GlobalExceptionHandlerMiddleware Create(RequestDelegate next)
        => new(next, _logger.Object, _env.Object);

    private static DefaultHttpContext CreateContext()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        return ctx;
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_CallsNextDelegate()
    {
        var nextCalled = false;
        var middleware = Create(_ => { nextCalled = true; return Task.CompletedTask; });

        await middleware.InvokeAsync(CreateContext());

        nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_WhenNoException_DoesNotModifyStatusCode()
    {
        var middleware = Create(_ => Task.CompletedTask);
        var context = CreateContext();

        await middleware.InvokeAsync(context);

        // Default HttpContext status is 200
        context.Response.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_ReturnsStatus500()
    {
        var middleware = Create(_ => throw new InvalidOperationException("boom"));
        var context = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_SetsContentTypeToProblemJson()
    {
        var middleware = Create(_ => throw new InvalidOperationException("boom"));
        var context = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.ContentType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_LogsError()
    {
        var exception = new InvalidOperationException("boom");
        var middleware = Create(_ => throw exception);

        await middleware.InvokeAsync(CreateContext());

        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_InProduction_ResponseBodyDoesNotContainExceptionMessage()
    {
        _env.Setup(e => e.EnvironmentName).Returns("Production");
        var middleware = Create(_ => throw new InvalidOperationException("secret internal detail"));
        var context = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        body.Should().NotContain("secret internal detail");
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_InDevelopment_ResponseBodyContainsExceptionMessageInDetail()
    {
        _env.Setup(e => e.EnvironmentName).Returns("Development");
        var middleware = Create(_ => throw new InvalidOperationException("null reference at line 42"));
        var context = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("detail").GetString().Should().Be("null reference at line 42");
    }

    [Fact]
    public async Task InvokeAsync_WhenExceptionThrown_ResponseBodyContainsStatus500()
    {
        var middleware = Create(_ => throw new InvalidOperationException("boom"));
        var context = CreateContext();

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var json = JsonDocument.Parse(body).RootElement;
        json.GetProperty("status").GetInt32().Should().Be(500);
    }
}
