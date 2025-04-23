namespace Automotive.Marketplace.Application.Interfaces.Data.Repositories
{
    using Automotive.Marketplace.Domain.Entities;

    public interface IRefreshTokenRepository
    {
        public Task AddRefreshTokenAsync(RefreshToken refreshToken, CancellationToken cancellationToken);

        public Task<RefreshToken?> GetRefreshTokenByTokenAsync(string token, CancellationToken cancellationToken);
    }
}
