namespace Automotive.Marketplace.Application.Interfaces.Data.Repositories;

using Automotive.Marketplace.Domain.Entities;

public interface IListingRepository : IBaseRepository<Listing>
{
    Task<IList<Listing>> GetListingDetailsWithCarAsync(CancellationToken cancellationToken);
}
