namespace Automotive.Marketplace.Infrastructure.Repositories;

using Automotive.Marketplace.Application.Interfaces.Data.Repositories;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DbContext;
using Microsoft.EntityFrameworkCore;

public class ListingRepository(AutomotiveContext automotiveContext) : BaseRepository<Listing>(automotiveContext), IListingRepository
{
    public async Task<IList<Listing>> GetListingDetailsWithCarAsync(CancellationToken cancellationToken)
    {
        return await this.automotiveContext.Listings
            .Include(listing => listing.CarDetails)
            .ThenInclude(cardetails => cardetails.Car)
            .ThenInclude(car => car.Model)
            .ThenInclude(model => model.Make)
            .ToListAsync(cancellationToken);
    }
}
