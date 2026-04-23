using System.Security.Claims;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Server.Filters;

/// <summary>
/// Authorization filter that authenticates user and their permissions for protected endpoints.
/// Works with <see cref="ProtectAttribute"/> to secure API endpoints.
/// </summary>
/// <param name="requiredPermissions">Array of permissions where user needs at least one to access the endpoint.</param>
/// <param name="repository">Repository for database access to fetch user permissions.</param>
public class AuthorizationFilter(Permission[] requiredPermissions, IRepository repository) : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var userId = ExtractUserIdFromClaims(context);
        if (userId == Guid.Empty)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (requiredPermissions.Length == 0)
        {
            return;
        }

        var user = repository
            .AsQueryable<User>()
            .Where(user => user.Id == userId)
            .FirstOrDefault();

        if (!UserHasARequiredPermission(userId))
        {
            context.Result = new ForbidResult();
        }
    }

    private static Guid ExtractUserIdFromClaims(AuthorizationFilterContext context)
    {
        if (context.HttpContext.User.Identity?.IsAuthenticated != true)
        {
            return Guid.Empty;
        }

        var userIdClaim = context.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    private bool UserHasARequiredPermission(Guid userId)
    {
        var user = repository
            .AsQueryable<User>()
            .Include(user => user.UserPermissions)
            .Where(u => u.Id == userId)
            .FirstOrDefault();

        if (user == null)
        {
            return false;
        }

        return user.UserPermissions
            .Any(userPermission => requiredPermissions
                .Contains(userPermission.Permission));
    }
}