namespace Automotive.Marketplace.Application.Features.AuthFeatures.RevokeRefreshToken
{
    using Automotive.Marketplace.Domain.Entities;
    using MediatR;

    public sealed record RevokeRefreshTokenRequest(string oldRefreshToken, string newRefreshToken, Guid accountId, DateTime newExpiryDate) : IRequest;
}
