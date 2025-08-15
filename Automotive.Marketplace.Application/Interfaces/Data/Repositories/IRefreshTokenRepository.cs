namespace Automotive.Marketplace.Application.Interfaces.Data.Repositories;

using Automotive.Marketplace.Domain.Entities;

public interface IRefreshTokenRepository : IBaseRepository<RefreshToken>
{
    public Task<RefreshToken?> GetRefreshTokenByTokenAsync(string token, CancellationToken cancellationToken);
}
