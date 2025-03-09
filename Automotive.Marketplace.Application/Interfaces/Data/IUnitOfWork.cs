namespace Automotive.Marketplace.Application.Interfaces.Data
{
    using Automotive.Marketplace.Application.Interfaces.Data.Repositories;

    public interface IUnitOfWork : IDisposable
    {
        //TODO Add the rest of the repositories

        public IListingRepository ListingRepository { get; }

        public Task SaveAsync(CancellationToken cancellatioToken);
    }
}
