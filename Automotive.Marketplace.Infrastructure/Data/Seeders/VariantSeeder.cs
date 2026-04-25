using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Infrastructure.Data.Seeders;

public class VariantSeeder(AutomotiveContext context) : IDevelopmentSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Set<Variant>().AnyAsync(cancellationToken))
            return;

        var models = await context.Set<Model>()
            .OrderBy(m => m.Name)
            .Take(20)
            .ToListAsync(cancellationToken);
        var fuels = await context.Set<Fuel>().ToListAsync(cancellationToken);
        var transmissions = await context.Set<Transmission>().ToListAsync(cancellationToken);
        var bodyTypes = await context.Set<BodyType>().ToListAsync(cancellationToken);

        if (!models.Any() || !fuels.Any() || !transmissions.Any() || !bodyTypes.Any())
            return;

        var random = new Random(42);

        foreach (var model in models)
        {
            for (int i = 0; i < 3; i++)
            {
                var variant = new VariantBuilder()
                    .WithModel(model.Id)
                    .WithFuel(fuels[random.Next(fuels.Count)].Id)
                    .WithTransmission(transmissions[random.Next(transmissions.Count)].Id)
                    .WithBodyType(bodyTypes[random.Next(bodyTypes.Count)].Id)
                    .Build();

                await context.AddAsync(variant, cancellationToken);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
