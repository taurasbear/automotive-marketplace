using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Automotive.Marketplace.Server.Filters;

public class UnprocessableEntityExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is InvalidRefreshTokenException)
        {
            var errorResponse = new ErrorResponse
            {
                Message = "Invalid refresh token.",
                Type = "InvalidToken",
            };
            context.Result = new UnprocessableEntityObjectResult(errorResponse);
            context.ExceptionHandled = true;
        }
    }
}