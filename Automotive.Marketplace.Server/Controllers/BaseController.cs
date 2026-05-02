using System.Security.Claims;
using Automotive.Marketplace.Domain.Constants;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

[Route("[controller]/[action]")]
[ApiController]
public class BaseController : ControllerBase
{
    protected Guid UserId
    {
        get
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Guid.Empty;
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return string.IsNullOrWhiteSpace(userId) ? Guid.Empty : Guid.Parse(userId);
        }
    }

    protected IReadOnlyList<string> Permissions
    {
        get
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return [];
            }

            return User.FindAll(CustomClaimType.Permissions)
                .Select(claim => claim.Value)
                .ToList();
        }
    }
}