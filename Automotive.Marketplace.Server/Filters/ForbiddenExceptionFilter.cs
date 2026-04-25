using Automotive.Marketplace.Application.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Automotive.Marketplace.Server.Filters;

public class ForbiddenExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is UnauthorizedAccessException)
        {
            var errorResponse = new ErrorResponse
            {
                Message = "You do not have permission to access this resource.",
                Type = "Forbidden"
            };
            context.Result = new ObjectResult(errorResponse) { StatusCode = StatusCodes.Status403Forbidden };
            context.ExceptionHandled = true;
        }
    }
}
