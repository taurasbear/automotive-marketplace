namespace Automotive.Marketplace.Application.Features.AuthFeatures.RefreshToken;

using MediatR;

public sealed record RefreshTokenCommand : IRequest<RefreshTokenResponse>
{
    public string RefreshToken { get; set; } = string.Empty;
};
