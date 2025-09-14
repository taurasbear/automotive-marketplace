using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Infrastructure.Data.Seeders;

public class ListingSeeder(AutomotiveContext context) : IDevelopmentSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Set<Listing>()
            .AnyAsync(cancellationToken))
        {
            return;
        }

        var users = await context.Set<User>()
            .ToListAsync(cancellationToken);

        var cars = await context.Set<Car>()
            .ToListAsync(cancellationToken);

        for (int i = 0; i < cars.Count; i++)
        {
            var user = users
                .Skip(i % users.Count)
                .First();

            var listing = new ListingBuilder()
                .WithCar(cars[i].Id)
                .WithSeller(user.Id)
                .Build();

            await context.AddAsync(listing, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}