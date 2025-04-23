namespace Automotive.Marketplace.Infrastructure
{
    using Automotive.Marketplace.Application.Interfaces.Data;
    using Automotive.Marketplace.Application.Interfaces.Data.Repositories;
    using Automotive.Marketplace.Infrastructure.Data.DbContext;
    using Automotive.Marketplace.Infrastructure.Repositories;
    using System.Threading;
    using System.Threading.Tasks;

    public class UnitOfWork : IUnitOfWork
    {
        private bool disposed = false;

        private readonly AutomotiveContext automotiveContext;

        private IRefreshTokenRepository refreshTokenRepository = null!;

        public IRefreshTokenRepository RefreshTokenRepository
        {
            get
            {
                return refreshTokenRepository ??= new RefreshTokenRepository(automotiveContext);
            }
        }

        private IAccountRepository accountRepository = null!;

        public IAccountRepository AccountRepository
        {
            get
            {
                return accountRepository ??= new AccountRepository(automotiveContext);
            }
        }

        private IListingRepository listingRepository = null!;

        public IListingRepository ListingRepository
        {
            get
            {
                return listingRepository ??= new ListingRepository(automotiveContext);
            }
        }

        public UnitOfWork(AutomotiveContext automotiveContext)
        {
            this.automotiveContext = automotiveContext;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                this.automotiveContext.Dispose();
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public async Task SaveAsync(CancellationToken cancellatioToken)
        {
            await automotiveContext.SaveChangesAsync(cancellatioToken);
        }
    }
}
