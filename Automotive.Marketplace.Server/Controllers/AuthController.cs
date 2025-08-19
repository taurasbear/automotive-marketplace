namespace Automotive.Marketplace.Server.Controllers;

using Automotive.Marketplace.Application.Features.AuthFeatures.AuthenticateAccount;
using Automotive.Marketplace.Application.Features.AuthFeatures.RefreshToken;
using Automotive.Marketplace.Application.Features.AuthFeatures.RegisterAccount;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class AuthController(IMediator mediator) : ControllerBase
{
    private readonly IMediator mediator = mediator;

    [HttpPost("login")]
    public async Task<ActionResult> Login(
        [FromBody] AuthenticateAccountCommand authenticateAccountRequest,
        CancellationToken cancellationToken)
    {
        var response = await this.mediator.Send(authenticateAccountRequest, cancellationToken);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = response.FreshExpiryDate,
            Secure = true,
            SameSite = SameSiteMode.Strict
        };

        Response.Cookies.Append("refreshToken", response.FreshRefreshToken, cookieOptions);

        return this.Ok(new
        {
            AccessToken = response.FreshAccessToken,
            AccountId = response.AccountId,
            Role = response.RoleName
        });
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterAccountCommand registerAccountRequest, CancellationToken cancellationToken)
    {
        var response = await this.mediator.Send(registerAccountRequest, cancellationToken);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = response.RefreshTokenExpiryDate,
            Secure = true,
            SameSite = SameSiteMode.Strict
        };

        Response.Cookies.Append("refreshToken", response.RefreshToken, cookieOptions);

        return this.Ok(new
        {
            AccessToken = response.AccessToken,
            AccountId = response.AccountId,
            Role = response.RoleName
        });
    }

    [HttpPost("refresh")]
    public async Task<ActionResult> Refresh(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return this.Unauthorized("Invalid refresh token.");
        }

        var response = await this.mediator.Send(new RefreshTokenRequest(refreshToken), cancellationToken);

        Response.Cookies.Append("refreshToken", response.FreshRefreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = response.FreshExpiryDate
        });

        return this.Ok(new
        {
            AccessToken = response.FreshAccessToken
        });
    }
}