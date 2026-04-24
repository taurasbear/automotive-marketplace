using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Automotive.Marketplace.Server.Filters;

public class ValidationExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is RequestValidationException validationException)
        {
            var errorResponse = new ErrorResponse
            {
                Messages = validationException.Errors.ToDictionary
                (
                    kvp => kvp.Key,
                    kvp => kvp.Value
                ),
                Type = "Validation"
            };
            context.Result = new BadRequestObjectResult(errorResponse);
            context.ExceptionHandled = true;
        }
    }
}