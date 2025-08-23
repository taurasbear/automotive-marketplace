using Automotive.Marketplace.Application.Features.AuthFeatures.AuthenticateAccount;
using Automotive.Marketplace.Application.Features.AuthFeatures.RefreshToken;
using Automotive.Marketplace.Application.Features.AuthFeatures.RegisterAccount;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Automotive.Marketplace.Server.Controllers;

public class AuthController(IMediator mediator) : BaseController
{
    [HttpPost]
    public async Task<ActionResult> Login(
        [FromBody] AuthenticateAccountCommand authenticateAccountRequest,
        CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return Ok();
        }

        var response = await mediator.Send(authenticateAccountRequest, cancellationToken);

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
            AccountId = response.AccountId,
            Role = response.RoleName
        });
    }

    [HttpPost]
    public async Task<ActionResult> Register(RegisterAccountCommand registerAccountRequest, CancellationToken cancellationToken)
    {
        var response = await mediator.Send(registerAccountRequest, cancellationToken);

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
            AccountId = response.AccountId,
            Role = response.RoleName
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
            AccessToken = response.FreshAccessToken
        });
    }

    [HttpPost]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["refreshToken"];
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            var command = new LogoutAccountCommand { RefreshToken = refreshToken };
            await mediator.Send(command, cancellationToken);
        }

        Response.Cookies.Delete("refreshToken");

        return Ok();
    }
}