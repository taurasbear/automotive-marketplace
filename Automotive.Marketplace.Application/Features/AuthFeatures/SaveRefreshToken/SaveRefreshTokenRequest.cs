namespace Automotive.Marketplace.Application.Features.AuthFeatures.SaveRefreshToken;

using MediatR;

public sealed record SaveRefreshTokenRequest(Guid accountId, string refreshToken, DateTime expiryDate) : IRequest;
