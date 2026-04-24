using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Server.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Attributes;

public class ProtectAttribute : TypeFilterAttribute
{
    /// <summary>
    /// Authenticates user and checks if they have any of the required permissions.
    /// </summary>
    /// <param name="permissions">Array of permissions where user needs at least one to access the endpoint.</param>
    public ProtectAttribute(params Permission[] permissions) : base(typeof(AuthorizationFilter))
    {
        Arguments = [permissions];
    }
};