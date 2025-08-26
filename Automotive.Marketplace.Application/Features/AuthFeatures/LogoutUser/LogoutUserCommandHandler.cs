using Automotive.Marketplace.Application.Interfaces.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RefreshTokenEntity = Automotive.Marketplace.Domain.Entities.RefreshToken;

namespace Automotive.Marketplace.Application.Features.AuthFeatures.LogoutUser;

public class LogoutUserCommandHandler(IRepository repository) : IRequestHandler<LogoutUserCommand>
{
    public async Task Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await repository
            .AsQueryable<RefreshTokenEntity>()
            .Where(refreshToken => refreshToken.Token == request.RefreshToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (refreshToken == null
            || string.IsNullOrWhiteSpace(refreshToken.Token)
            || refreshToken.IsRevoked
            || refreshToken.ExpiryDate < DateTime.UtcNow)
        {
            return;
        }

        refreshToken.IsRevoked = true;
        await repository.UpdateAsync(refreshToken, cancellationToken);

        return;
    }
}
