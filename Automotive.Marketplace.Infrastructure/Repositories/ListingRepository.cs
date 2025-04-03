namespace Automotive.Marketplace.Infrastructure.Repositories
{
    using Automotive.Marketplace.Application.Interfaces.Data.Repositories;
    using Automotive.Marketplace.Domain.Entities;
    using Automotive.Marketplace.Infrastructure.Data.DbContext;
    using Microsoft.EntityFrameworkCore;

    public class ListingRepository : BaseRepository, IListingRepository
    {
        public ListingRepository(AutomotiveContext automotiveContext) : base(automotiveContext)
        { }

        public async Task<IList<Listing>> GetListingDetailsWithCar(CancellationToken cancellationToken)
        {
            return await this.automotiveContext.Listings
                .Include(listing => listing.CarDetails)
                .ThenInclude(cardetails => cardetails.Car)
                .ThenInclude(car => car.Model)
                .ToListAsync(cancellationToken);
        }
    }
}
