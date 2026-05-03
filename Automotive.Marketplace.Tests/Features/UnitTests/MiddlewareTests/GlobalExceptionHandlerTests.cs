using System.Text.Json;
using Automotive.Marketplace.Server.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Automotive.Marketplace.Tests.Features.UnitTests.MiddlewareTests;

public class GlobalExceptionHandlerTests
{
    private readonly IHostEnvironment _env;
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly GlobalExceptionHandler _handler;

    public GlobalExceptionHandlerTests()
    {
        _env = Substitute.For<IHostEnvironment>();
        _logger = Substitute.For<ILogger<GlobalExceptionHandler>>();
        _handler = new GlobalExceptionHandler(_env, _logger);
    }

    [Fact]
    public async Task TryHandleAsync_ReturnsTrue()
    {
        // Arrange
        _env.EnvironmentName.Returns("Production");
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        httpContext.Response.StatusCode = 500;
        var exception = new Exception("Test error");

        // Act
        var result = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TryHandleAsync_SetsContentType()
    {
        // Arrange
        _env.EnvironmentName.Returns("Production");
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        httpContext.Response.StatusCode = 500;
        var exception = new Exception("Test error");

        // Act
        await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        httpContext.Response.ContentType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task TryHandleAsync_WritesJsonResponse()
    {
        // Arrange
        _env.EnvironmentName.Returns("Production");
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        httpContext.Response.StatusCode = 500;
        var exception = new Exception("Test error");

        // Act
        await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(
            responseBody,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)
        );

        problemDetails.Should().NotBeNull();
        problemDetails!.Status.Should().Be(500);
        problemDetails.Title.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task TryHandleAsync_InDevelopment_IncludesExceptionDetails()
    {
        // Arrange
        _env.EnvironmentName.Returns("Development");
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        httpContext.Response.StatusCode = 500;
        var exception = new Exception("Test error message");

        // Act
        await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(
            responseBody,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)
        );

        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().NotBeNullOrEmpty();
        problemDetails.Detail.Should().Contain("Test error message");
        problemDetails.Extensions.Should().ContainKey("traceId");
        problemDetails.Extensions.Should().ContainKey("requestId");
        problemDetails.Extensions.Should().ContainKey("data");
    }

    [Fact]
    public async Task TryHandleAsync_InProduction_ExcludesExceptionDetails()
    {
        // Arrange
        _env.EnvironmentName.Returns("Production");
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        httpContext.Response.StatusCode = 500;
        var exception = new Exception("Test error message");

        // Act
        await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Assert
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(httpContext.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(
            responseBody,
            new JsonSerializerOptions(JsonSerializerDefaults.Web)
        );

        problemDetails.Should().NotBeNull();
        problemDetails!.Detail.Should().BeNull();
        problemDetails.Extensions.Should().BeEmpty();
    }
}
