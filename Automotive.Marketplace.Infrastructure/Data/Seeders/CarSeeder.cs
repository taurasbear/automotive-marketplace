using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Automotive.Marketplace.Infrastructure.Data.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Seeders;

public class CarSeeder(AutomotiveContext context) : IDevelopmentSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Set<Car>()
            .AnyAsync(cancellationToken))
        {
            return;
        }

        var models = await context.Set<Model>()
            .ToListAsync(cancellationToken);

        foreach (var model in models)
        {
            var cars = new CarBuilder()
                .WithModel(model.Id)
                .Build(3);

            await context.AddRangeAsync(cars, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}