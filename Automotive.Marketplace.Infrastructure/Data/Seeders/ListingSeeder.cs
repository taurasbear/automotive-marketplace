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
        if (await context.Set<Listing>().AnyAsync(cancellationToken))
            return;

        var users = await context.Set<User>().ToListAsync(cancellationToken);
        var variants = await context.Set<Variant>().ToListAsync(cancellationToken);
        var drivetrains = await context.Set<Drivetrain>().ToListAsync(cancellationToken);

        if (!users.Any() || !variants.Any() || !drivetrains.Any())
            return;

        for (int i = 0; i < variants.Count; i++)
        {
            var user = users.Skip(i % users.Count).First();
            var drivetrain = drivetrains[i % drivetrains.Count];

            var listing = new ListingBuilder()
                .WithVariant(variants[i].Id)
                .WithSeller(user.Id)
                .WithDrivetrain(drivetrain.Id)
                .Build();

            await context.AddAsync(listing, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}