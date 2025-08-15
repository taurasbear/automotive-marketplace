namespace Automotive.Marketplace.Infrastructure.Repositories;

using Automotive.Marketplace.Application.Interfaces.Data.Repositories;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;

public class RefreshTokenRepository(
    AutomotiveContext automotiveContext) : BaseRepository<RefreshToken>(automotiveContext), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetRefreshTokenByTokenAsync(string token, CancellationToken cancellationToken)
    {
        return await this.automotiveContext.RefreshTokens
            .Where(refreshToken => refreshToken.Token == token)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
