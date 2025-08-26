using Automotive.Marketplace.Application.Features.AuthFeatures.LoginUser;
using Automotive.Marketplace.Application.Features.AuthFeatures.LogoutUser;
using Automotive.Marketplace.Application.Features.AuthFeatures.RefreshToken;
using Automotive.Marketplace.Application.Features.AuthFeatures.RegisterUser;
using Automotive.Marketplace.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

public class AuthController(IMediator mediator) : BaseController
{
    [HttpPost]
    public async Task<ActionResult> Login(
        [FromBody] LoginUserCommand command,
        CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Ok();
        }

        var response = await mediator.Send(command, cancellationToken);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = response.FreshExpiryDate,
            Secure = true,
            SameSite = SameSiteMode.Strict
        };

        Response.Cookies.Append("refreshToken", response.FreshRefreshToken, cookieOptions);

        return Ok(new
        {
            AccessToken = response.FreshAccessToken,
            UserId = response.UserId,
            Permissions = response.Permissions,
        });
    }

    [HttpPost]
    public async Task<ActionResult> Register(RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(command, cancellationToken);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = response.RefreshTokenExpiryDate,
            Secure = true,
            SameSite = SameSiteMode.Strict
        };

        Response.Cookies.Append("refreshToken", response.RefreshToken, cookieOptions);

        return Ok(new
        {
            AccessToken = response.AccessToken,
            UserId = response.UserId,
        });
    }

    [HttpPost]
    public async Task<ActionResult> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized("Invalid refresh token.");
        }

        var command = new RefreshTokenCommand { RefreshToken = refreshToken };
        var response = await mediator.Send(command, cancellationToken);

        Response.Cookies.Append("refreshToken", response.FreshRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = response.FreshExpiryDate
        });

        return Ok(new
        {
            AccessToken = response.FreshAccessToken,
            UserId = response.UserId,
            Permissions = response.Permissions,
        });
    }

    [HttpPost]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var command = new LogoutUserCommand { RefreshToken = refreshToken };
            await mediator.Send(command, cancellationToken);
        }

        Response.Cookies.Delete("refreshToken");

        return Ok();
    }
}