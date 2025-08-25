using AutoMapper;
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RefreshTokenEntity = Automotive.Marketplace.Domain.Entities.RefreshToken;

namespace Automotive.Marketplace.Application.Features.AuthFeatures.RefreshToken;

public class RefreshTokenCommandHandler(
    IMapper mapper,
    ITokenService tokenService,
    IRepository repository) : IRequestHandler<RefreshTokenCommand, RefreshTokenResponse>
{
    public async Task<RefreshTokenResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var currentRefreshToken = await repository
            .AsQueryable<RefreshTokenEntity>()
            .Where(refreshToken => refreshToken.Token == request.RefreshToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (currentRefreshToken == null
            || string.IsNullOrWhiteSpace(currentRefreshToken.Token)
            || currentRefreshToken.IsRevoked
            || currentRefreshToken.ExpiryDate < DateTime.UtcNow)
        {
            throw new InvalidRefreshTokenException();
        }

        var fetchedAccount = await repository.GetByIdAsync<User>(currentRefreshToken.UserId, cancellationToken)
            ?? throw new UserNotFoundException(currentRefreshToken.UserId);

        currentRefreshToken.IsRevoked = true;

        var freshAccessToken = tokenService.GenerateAccessToken(fetchedAccount);
        var freshRefreshToken = tokenService.GenerateRefreshTokenEntity(fetchedAccount);

        await repository.CreateAsync(freshRefreshToken, cancellationToken);

        var response = mapper.Map<RefreshTokenResponse>(freshRefreshToken);
        response.FreshAccessToken = freshAccessToken;

        return response;
    }
}
