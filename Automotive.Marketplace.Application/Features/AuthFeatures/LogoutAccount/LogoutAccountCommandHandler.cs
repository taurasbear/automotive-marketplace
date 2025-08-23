using Automotive.Marketplace.Application.Interfaces.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RefreshTokenEntity = Automotive.Marketplace.Domain.Entities.RefreshToken;

namespace Automotive.Marketplace.Application.Features.AuthFeatures.RefreshToken;

public class LogoutAccountCommandHandler(IRepository repository) : IRequestHandler<LogoutAccountCommand>
{
    public async Task Handle(LogoutAccountCommand request, CancellationToken cancellationToken)
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
