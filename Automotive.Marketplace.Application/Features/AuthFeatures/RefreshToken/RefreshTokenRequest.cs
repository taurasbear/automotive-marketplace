namespace Automotive.Marketplace.Application.Features.AuthFeatures.RefreshToken;

using MediatR;

public sealed record RefreshTokenRequest(string refreshToken) : IRequest<RefreshTokenResponse>;
