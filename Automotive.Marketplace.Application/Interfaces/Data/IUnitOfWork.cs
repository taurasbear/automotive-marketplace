namespace Automotive.Marketplace.Application.Interfaces.Data
{
    using Automotive.Marketplace.Application.Interfaces.Data.Repositories;

    public interface IUnitOfWork : IDisposable
    {
        public IRefreshTokenRepository RefreshTokenRepository { get; }

        public IAccountRepository AccountRepository { get; }

        public IListingRepository ListingRepository { get; }

        public Task SaveAsync(CancellationToken cancellatioToken);
    }
}
