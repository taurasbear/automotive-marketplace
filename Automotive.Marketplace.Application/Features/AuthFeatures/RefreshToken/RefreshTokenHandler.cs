namespace Automotive.Marketplace.Application.Features.AuthFeatures.RefreshToken;

using AutoMapper;
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RefreshTokenEntity = Domain.Entities.RefreshToken;
public class RefreshTokenHandler(
    IMapper mapper,
    ITokenService tokenService,
    IRepository repository) : IRequestHandler<RefreshTokenRequest, RefreshTokenResponse>
{
    public async Task<RefreshTokenResponse> Handle(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var currentRefreshToken = await repository
            .AsQueryable<RefreshTokenEntity>()
            .Where(refreshToken => refreshToken.Token == request.refreshToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentRefreshToken == null
            || string.IsNullOrWhiteSpace(currentRefreshToken.Token)
            || currentRefreshToken.IsRevoked
            || currentRefreshToken.ExpiryDate < DateTime.UtcNow)
        {
            throw new InvalidRefreshTokenException();
        }

        var fetchedAccount = await repository.GetByIdAsync<Account>(currentRefreshToken.AccountId, cancellationToken)
            ?? throw new AccountNotFoundException(currentRefreshToken.AccountId);

        currentRefreshToken.IsRevoked = true;

        var freshAccessToken = tokenService.GenerateAccessToken(fetchedAccount);
        var freshRefreshToken = tokenService.GenerateRefreshTokenEntity(fetchedAccount);

        await repository.CreateAsync(freshRefreshToken, cancellationToken);

        var response = mapper.Map<RefreshTokenResponse>(freshRefreshToken);
        response.FreshAccessToken = freshAccessToken;

        return response;
    }
}
