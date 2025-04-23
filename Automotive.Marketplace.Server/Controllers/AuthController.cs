namespace Automotive.Marketplace.Server.Controllers
{
    using Automotive.Marketplace.Application.Features.AccountFeatures.GetAccountById;
    using Automotive.Marketplace.Application.Features.AuthFeatures.AuthenticateAccount;
    using Automotive.Marketplace.Application.Features.AuthFeatures.GetRefreshTokenByToken;
    using Automotive.Marketplace.Application.Features.AuthFeatures.RegisterAccount;
    using Automotive.Marketplace.Application.Features.AuthFeatures.RevokeRefreshToken;
    using Automotive.Marketplace.Application.Features.AuthFeatures.SaveRefreshToken;
    using Automotive.Marketplace.Application.Interfaces.Services;
    using MediatR;
    using Microsoft.AspNetCore.Mvc;

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService tokenService;

        private readonly IMediator mediator;

        public AuthController(ITokenService tokenService, IMediator mediator)
        {
            this.tokenService = tokenService;
            this.mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] AuthenticateAccountRequest authenticateAccountRequest, CancellationToken cancellationToken)
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

            var refreshTokenResponse = await this.mediator.Send(new GetRefreshTokenByTokenRequest(refreshToken), cancellationToken);
            var refreshTokenRecord = refreshTokenResponse.RefreshToken;

            if (refreshTokenRecord == null || refreshTokenRecord.IsRevoked || refreshTokenRecord.ExpiryDate < DateTime.UtcNow)
            {
                return this.Unauthorized("Invalid refresh token.");
            }

            var accountResponse = await this.mediator.Send(new GetAccountByIdRequest(refreshTokenRecord.AccountId));

            if (accountResponse.Account == null)
            {
                return this.Unauthorized("Account not found.");
            }

            var newAccessToken = this.tokenService.GenerateAccessToken(accountResponse.Account);
            var newRefreshToken = this.tokenService.GenerateRefreshToken();
            var newRefreshTokenExpiryDate = this.tokenService.GetRefreshTokenExpiryData();

            await this.mediator.Send(new RevokeRefreshTokenRequest(refreshTokenRecord.Token, newRefreshToken, refreshTokenRecord.AccountId, newRefreshTokenExpiryDate));

            Response.Cookies.Append("refreshToken", newRefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = newRefreshTokenExpiryDate
            });

            return this.Ok(new { AccessToken = newAccessToken });
        }
    }
}