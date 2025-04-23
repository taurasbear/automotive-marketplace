namespace Automotive.Marketplace.Application.Features.AuthFeatures.GetRefreshTokenByToken
{
    using Automotive.Marketplace.Domain.Entities;

    public sealed record GetRefreshTokenByTokenResponse
    {
        public RefreshToken? RefreshToken { get; set; }
    }
}
