using MediatR;

namespace Automotive.Marketplace.Application.Features.AuthFeatures.RefreshToken;

public sealed record RefreshTokenCommand : IRequest<RefreshTokenResponse>
{
    public string RefreshToken { get; set; } = string.Empty;
};
