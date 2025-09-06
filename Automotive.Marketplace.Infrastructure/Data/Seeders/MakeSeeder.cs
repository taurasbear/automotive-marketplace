using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Infrastructure.Data.Seeders;

public class MakeSeeder(AutomotiveContext context) : IDevelopmentSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Set<Make>()
            .AnyAsync(cancellationToken))
        {
            return;
        }

        var makes = new MakeBuilder().Build(3);

        await context.AddRangeAsync(makes, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}