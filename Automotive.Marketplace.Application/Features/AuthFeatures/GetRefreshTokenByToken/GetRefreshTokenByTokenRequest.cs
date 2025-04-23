namespace Automotive.Marketplace.Application.Features.AuthFeatures.GetRefreshTokenByToken
{
    using MediatR;

    public sealed record GetRefreshTokenByTokenRequest(string token) : IRequest<GetRefreshTokenByTokenResponse>;
}
