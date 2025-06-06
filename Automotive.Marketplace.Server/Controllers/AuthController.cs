namespace Automotive.Marketplace.Server.Controllers;

using Automotive.Marketplace.Application.Features.AuthFeatures.AuthenticateAccount;
using Automotive.Marketplace.Application.Features.AuthFeatures.RefreshToken;
using Automotive.Marketplace.Application.Features.AuthFeatures.RegisterAccount;
using Automotive.Marketplace.Application.Features.AuthFeatures.SaveRefreshToken;
using Automotive.Marketplace.Application.Interfaces.Services;
using MediatR;
using Microsoft.AspNetCore.Mvc;

[Route("[controller]")]
[ApiController]
public class AuthController(ITokenService tokenService, IMediator mediator) : ControllerBase
{
    private readonly ITokenService tokenService = tokenService;

    private readonly IMediator mediator = mediator;

    [HttpPost("login")]
    public async Task<ActionResult> Login(
        [FromBody] AuthenticateAccountRequest authenticateAccountRequest,
        CancellationToken cancellationToken)
    {
        var response = await this.mediator.Send(authenticateAccountRequest, cancellationToken);

        if (response.Account == null)
        {
            return this.Unauthorized("Account not found.");
        }

        var accessToken = this.tokenService.GenerateAccessToken(response.Account);
        var refreshToken = this.tokenService.GenerateRefreshToken();
        var refreshTokenExpiryDate = this.tokenService.GetRefreshTokenExpiryData();

        await this.mediator.Send(
            new SaveRefreshTokenRequest(response.Account.Id, refreshToken, refreshTokenExpiryDate));

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = refreshTokenExpiryDate,
            Secure = true,
            SameSite = SameSiteMode.Strict
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

        return this.Ok(new
        {
            AccessToken = accessToken,
            AccountId = response.Account.Id,
            Role = response.Account.RoleName
        });
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterAccountRequest registerAccountRequest, CancellationToken cancellationToken)
    {
        var response = await this.mediator.Send(registerAccountRequest, cancellationToken);

        var accessToken = this.tokenService.GenerateAccessToken(response.Account);
        var refreshToken = this.tokenService.GenerateRefreshToken();
        var refreshTokenExpiryDate = this.tokenService.GetRefreshTokenExpiryData();

        await this.mediator.Send(
            new SaveRefreshTokenRequest(response.Account.Id, refreshToken, refreshTokenExpiryDate));

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = refreshTokenExpiryDate,
            Secure = true,
            SameSite = SameSiteMode.Strict
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

        return this.Ok(new
        {
            AccessToken = accessToken,
            AccountId = response.Account.Id,
            Role = response.Account.RoleName
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

        return this.Ok(new { AccessToken = response.FreshAccessToken });
    }
}