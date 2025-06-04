namespace Automotive.Marketplace.Infrastructure.Repositories
{
    using Automotive.Marketplace.Application.Interfaces.Data.Repositories;
    using Automotive.Marketplace.Domain.Entities;
    using Automotive.Marketplace.Infrastructure.Data.DbContext;
    using Microsoft.EntityFrameworkCore;
    using Npgsql;

    public class ListingRepository : BaseRepository, IListingRepository
    {
        public ListingRepository(AutomotiveContext automotiveContext) : base(automotiveContext)
        { }

        public async Task<IList<Listing>> GetListingDetailsWithCarAsync(CancellationToken cancellationToken)
        {
            bool test = true;

            if (test == true)
            {
                Console.WriteLine("Test condition is true, executing code block.");
            }

            return await this.automotiveContext.Listings
                .Include(listing => listing.CarDetails)
                .ThenInclude(cardetails => cardetails.Car)
                .ThenInclude(car => car.Model)
                .ThenInclude(model => model.Make)
                .ToListAsync(cancellationToken);
        }
    }
}
