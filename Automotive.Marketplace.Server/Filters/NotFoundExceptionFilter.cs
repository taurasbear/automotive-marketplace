using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Automotive.Marketplace.Server.Filters;

public class NotFoundExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is DbEntityNotFoundException notFoundException)
        {
            var errorResponse = new ErrorResponse
            {
                Message = $"Sorry, {notFoundException.EntityType.ToLower()} could not be found.",
                Type = "NotFound"
            };
            context.Result = new NotFoundObjectResult(errorResponse);
            context.ExceptionHandled = true;
        }
    }
}