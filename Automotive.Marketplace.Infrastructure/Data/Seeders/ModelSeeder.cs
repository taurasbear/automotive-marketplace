using Automotive.Marketplace.Domain.Entities;
using ModelEntityBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Infrastructure.Data.Seeders;

public class ModelSeeder(AutomotiveContext context) : IDevelopmentSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Set<Model>()
            .AnyAsync(cancellationToken))
        {
            return;
        }

        var makes = await context.Set<Make>()
            .ToListAsync(cancellationToken);

        foreach (var make in makes)
        {
            var models = new ModelEntityBuilder()
                .WithMake(make.Id)
                .Build(3);

            await context.AddRangeAsync(models, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}