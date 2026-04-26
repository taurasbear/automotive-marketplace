using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Infrastructure.Data.Seeders;

public class ListingSeeder(AutomotiveContext context) : IDevelopmentSeeder
{
    private const int ListingsPerVariant = 3;

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Set<Listing>().AnyAsync(cancellationToken))
            return;

        var users = await context.Set<User>().ToListAsync(cancellationToken);
        var variants = await context.Set<Variant>()
            .Include(v => v.Model).ThenInclude(m => m.Make)
            .ToListAsync(cancellationToken);
        var drivetrains = await context.Set<Drivetrain>().ToListAsync(cancellationToken);
        var municipalities = await context.Set<Municipality>().ToListAsync(cancellationToken);

        if (!users.Any() || !variants.Any() || !drivetrains.Any() || !municipalities.Any())
            return;

        var fwd = drivetrains.First(d => d.Name == "FWD");
        var rwd = drivetrains.First(d => d.Name == "RWD");
        var awd = drivetrains.First(d => d.Name == "AWD");

        var random = new Random(42);
        var currentYear = DateTime.Now.Year;
        var listingIndex = 0;

        foreach (var variant in variants)
        {
            var makeName = variant.Model.Make.Name;
            var drivetrain = makeName == "Subaru" ? awd
                : variant.PowerKw >= 220 ? rwd
                : fwd;

            for (var i = 0; i < ListingsPerVariant; i++)
            {
                var year = random.Next(2008, currentYear + 1);
                var age = currentYear - year;
                var mileage = Math.Clamp(
                    age * random.Next(8000, 22000) + random.Next(-3000, 3000),
                    min: 2000, max: 380000);
                var basePrice = (year - 2004) * 700 + variant.PowerKw * 25;
                var priceMultiplier = (decimal)(0.8 + random.NextDouble() * 0.4);
                var price = Math.Max((decimal)basePrice * priceMultiplier, 800m);

                var user = users[listingIndex % users.Count];
                var municipality = municipalities[listingIndex % municipalities.Count];

                var listing = new ListingBuilder()
                    .WithVariant(variant.Id)
                    .WithSeller(user.Id)
                    .WithDrivetrain(drivetrain.Id)
                    .WithMunicipality(municipality.Id)
                    .WithYear(year)
                    .WithMileage(mileage)
                    .WithPrice(price)
                    .Build();

                await context.AddAsync(listing, cancellationToken);
                listingIndex++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}