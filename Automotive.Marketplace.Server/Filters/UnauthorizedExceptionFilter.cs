using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Automotive.Marketplace.Server.Filters;

public class UnauthorizedExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is InvalidCredentialsException || context.Exception is UserNotFoundException)
        {
            var errorResponse = new ErrorResponse
            {
                Message = "Invalid email or password.",
                Type = "Authentication"
            };
            context.Result = new UnauthorizedObjectResult(errorResponse);
            context.ExceptionHandled = true;
        }
        else if (context.Exception is MissingRefreshTokenException)
        {
            var errorResponse = new ErrorResponse
            {
                Message = "No refresh token was provided.",
                Type = "MissingToken"
            };
            context.Result = new UnauthorizedObjectResult(errorResponse);
            context.ExceptionHandled = true;
        }
    }
}