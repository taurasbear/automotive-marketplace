namespace Automotive.Marketplace.Application.Interfaces.Data.Repositories
{
    using Automotive.Marketplace.Domain.Entities;

    public interface IListingRepository
    {
        Task<IList<Listing>> GetListingDetailsWithCarAsync(CancellationToken cancellationToken);
    }
}
