using System.Security.Claims;
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Models;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Server.Filters;
using FluentAssertions;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using NSubstitute;

namespace Automotive.Marketplace.Tests.Features.UnitTests.FilterTests;

public class ExceptionFilterTests
{
    #region ValidationExceptionFilter Tests

    [Fact]
    public void ValidationExceptionFilter_OnException_RequestValidationException_Returns400WithErrors()
    {
        // Arrange
        var filter = new ValidationExceptionFilter();
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email is required." } },
            { "Password", new[] { "Password must be at least 8 characters." } }
        };
        var validationException = new RequestValidationException(
            new[] {
                new ValidationFailure("Email", "Email is required."),
                new ValidationFailure("Password", "Password must be at least 8 characters.")
            }
        );
        var context = CreateExceptionContext(validationException);

        // Act
        filter.OnException(context);

        // Assert
        context.ExceptionHandled.Should().BeTrue();
        context.Result.Should().BeOfType<BadRequestObjectResult>();
        var result = context.Result as BadRequestObjectResult;
        result?.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        
        var errorResponse = result?.Value as ErrorResponse;
        errorResponse.Should().NotBeNull();
        errorResponse!.Type.Should().Be("Validation");
        errorResponse.Messages.Should().NotBeNull();
        errorResponse.Messages!.Keys.Should().Contain("Email", "Password");
        errorResponse.Messages["Email"].Should().Contain("Email is required.");
        errorResponse.Messages["Password"].Should().Contain("Password must be at least 8 characters.");
    }

    [Fact]
    public void ValidationExceptionFilter_OnException_OtherException_DoesNotHandle()
    {
        // Arrange
        var filter = new ValidationExceptionFilter();
        var argumentException = new ArgumentException("Invalid argument");
        var context = CreateExceptionContext(argumentException);

        // Act
        filter.OnException(context);

        // Assert
        context.ExceptionHandled.Should().BeFalse();
        context.Result.Should().BeNull();
    }

    #endregion

    #region NotFoundExceptionFilter Tests

    [Fact]
    public void NotFoundExceptionFilter_OnException_DbEntityNotFoundException_Returns404WithMessage()
    {
        // Arrange
        var filter = new NotFoundExceptionFilter();
        var entityId = Guid.NewGuid();
        var notFoundException = new DbEntityNotFoundException("Car", entityId);
        var context = CreateExceptionContext(notFoundException);

        // Act
        filter.OnException(context);

        // Assert
        context.ExceptionHandled.Should().BeTrue();
        context.Result.Should().BeOfType<NotFoundObjectResult>();
        var result = context.Result as NotFoundObjectResult;
        result?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        
        var errorResponse = result?.Value as ErrorResponse;
        errorResponse.Should().NotBeNull();
        errorResponse!.Type.Should().Be("NotFound");
        errorResponse.Message.Should().Be("Sorry, car could not be found.");
    }

    [Fact]
    public void NotFoundExceptionFilter_OnException_OtherException_DoesNotHandle()
    {
        // Arrange
        var filter = new NotFoundExceptionFilter();
        var invalidOperationException = new InvalidOperationException("Invalid operation");
        var context = CreateExceptionContext(invalidOperationException);

        // Act
        filter.OnException(context);

        // Assert
        context.ExceptionHandled.Should().BeFalse();
        context.Result.Should().BeNull();
    }

    #endregion

    #region UnauthorizedExceptionFilter Tests

    [Fact]
    public void UnauthorizedExceptionFilter_OnException_InvalidCredentialsException_Returns401()
    {
        // Arrange
        var filter = new UnauthorizedExceptionFilter();
        var invalidCredentialsException = new InvalidCredentialsException();
        var context = CreateExceptionContext(invalidCredentialsException);

        // Act
        filter.OnException(context);

        // Assert
        context.ExceptionHandled.Should().BeTrue();
        context.Result.Should().BeOfType<UnauthorizedObjectResult>();
        var result = context.Result as UnauthorizedObjectResult;
        result?.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        
        var errorResponse = result?.Value as ErrorResponse;
        errorResponse.Should().NotBeNull();
        errorResponse!.Type.Should().Be("Authentication");
        errorResponse.Message.Should().Be("Invalid email or password.");
    }

    [Fact]
    public void UnauthorizedExceptionFilter_OnException_UserNotFoundException_Returns401()
    {
        // Arrange
        var filter = new UnauthorizedExceptionFilter();
        var userNotFoundException = new UserNotFoundException("test@example.com");
        var context = CreateExceptionContext(userNotFoundException);

        // Act
        filter.OnException(context);

        // Assert
        context.ExceptionHandled.Should().BeTrue();
        context.Result.Should().BeOfType<UnauthorizedObjectResult>();
        var result = context.Result as UnauthorizedObjectResult;
        result?.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        
        var errorResponse = result?.Value as ErrorResponse;
        errorResponse.Should().NotBeNull();
        errorResponse!.Type.Should().Be("Authentication");
        errorResponse.Message.Should().Be("Invalid email or password.");
    }

    [Fact]
    public void UnauthorizedExceptionFilter_OnException_MissingRefreshTokenException_Returns401()
    {
        // Arrange
        var filter = new UnauthorizedExceptionFilter();
        var missingRefreshTokenException = new MissingRefreshTokenException();
        var context = CreateExceptionContext(missingRefreshTokenException);

        // Act
        filter.OnException(context);

        // Assert
        context.ExceptionHandled.Should().BeTrue();
        context.Result.Should().BeOfType<UnauthorizedObjectResult>();
        var result = context.Result as UnauthorizedObjectResult;
        result?.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        
        var errorResponse = result?.Value as ErrorResponse;
        errorResponse.Should().NotBeNull();
        errorResponse!.Type.Should().Be("MissingToken");
        errorResponse.Message.Should().Be("No refresh token was provided.");
    }

    [Fact]
    public void UnauthorizedExceptionFilter_OnException_InvalidRefreshTokenException_Returns401()
    {
        // Arrange
        var filter = new UnauthorizedExceptionFilter();
        var invalidRefreshTokenException = new InvalidRefreshTokenException("invalid-token");
        var context = CreateExceptionContext(invalidRefreshTokenException);

        // Act
        filter.OnException(context);

        // Assert
        context.ExceptionHandled.Should().BeTrue();
        context.Result.Should().BeOfType<UnauthorizedObjectResult>();
        var result = context.Result as UnauthorizedObjectResult;
        result?.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        
        var errorResponse = result?.Value as ErrorResponse;
        errorResponse.Should().NotBeNull();
        errorResponse!.Type.Should().Be("InvalidToken");
        errorResponse.Message.Should().Be("The provided refresh token is invalid or expired.");
    }

    [Fact]
    public void UnauthorizedExceptionFilter_OnException_OtherException_DoesNotHandle()
    {
        // Arrange
        var filter = new UnauthorizedExceptionFilter();
        var timeoutException = new TimeoutException("Request timed out");
        var context = CreateExceptionContext(timeoutException);

        // Act
        filter.OnException(context);

        // Assert
        context.ExceptionHandled.Should().BeFalse();
        context.Result.Should().BeNull();
    }

    #endregion

    #region ForbiddenExceptionFilter Tests

    [Fact]
    public void ForbiddenExceptionFilter_OnException_UnauthorizedAccessException_Returns403()
    {
        // Arrange
        var filter = new ForbiddenExceptionFilter();
        var unauthorizedAccessException = new UnauthorizedAccessException();
        var context = CreateExceptionContext(unauthorizedAccessException);

        // Act
        filter.OnException(context);

        // Assert
        context.ExceptionHandled.Should().BeTrue();
        context.Result.Should().BeOfType<ObjectResult>();
        var result = context.Result as ObjectResult;
        result?.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        
        var errorResponse = result?.Value as ErrorResponse;
        errorResponse.Should().NotBeNull();
        errorResponse!.Type.Should().Be("Forbidden");
        errorResponse.Message.Should().Be("You do not have permission to access this resource.");
    }

    [Fact]
    public void ForbiddenExceptionFilter_OnException_OtherException_DoesNotHandle()
    {
        // Arrange
        var filter = new ForbiddenExceptionFilter();
        var notSupportedException = new NotSupportedException("Operation not supported");
        var context = CreateExceptionContext(notSupportedException);

        // Act
        filter.OnException(context);

        // Assert
        context.ExceptionHandled.Should().BeFalse();
        context.Result.Should().BeNull();
    }

    #endregion

    #region AuthorizationFilter Tests

    [Fact]
    public void AuthorizationFilter_OnAuthorization_NoAuthenticatedUser_Returns401()
    {
        // Arrange
        var repositoryMock = Substitute.For<IRepository>();
        var authorizationFilter = new AuthorizationFilter(new[] { Permission.ViewListings }, repositoryMock);
        var context = CreateAuthorizationContext(null);

        // Act
        authorizationFilter.OnAuthorization(context);

        // Assert
        context.Result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public void AuthorizationFilter_OnAuthorization_NoRequiredPermissions_Passes()
    {
        // Arrange
        var repositoryMock = Substitute.For<IRepository>();
        var authorizationFilter = new AuthorizationFilter(Array.Empty<Permission>(), repositoryMock);
        
        var userId = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        var context = CreateAuthorizationContext(principal);

        // Act
        authorizationFilter.OnAuthorization(context);

        // Assert
        context.Result.Should().BeNull();
    }

    [Fact]
    public void AuthorizationFilter_OnAuthorization_UserWithPermission_Passes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var repositoryMock = Substitute.For<IRepository>();
        
        var userPermission = new UserPermission { Permission = Permission.ViewListings };
        var user = new User { Id = userId, UserPermissions = new List<UserPermission> { userPermission } };
        
        var queryableUsers = new[] { user }.AsQueryable();
        repositoryMock
            .AsQueryable<User>()
            .Returns(queryableUsers);

        var authorizationFilter = new AuthorizationFilter(new[] { Permission.ViewListings }, repositoryMock);
        
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        var context = CreateAuthorizationContext(principal);

        // Act
        authorizationFilter.OnAuthorization(context);

        // Assert
        context.Result.Should().BeNull();
    }

    #endregion

    #region Helper Methods

    private static ExceptionContext CreateExceptionContext(Exception exception)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new RouteData(),
            new ActionDescriptor());
        return new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = exception
        };
    }

    private static AuthorizationFilterContext CreateAuthorizationContext(ClaimsPrincipal? principal = null)
    {
        var httpContext = new DefaultHttpContext();
        if (principal != null) httpContext.User = principal;
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }

    #endregion
}
