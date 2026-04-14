using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Infrastructure.Data.Seeders;

public class FuelSeeder(AutomotiveContext context) : IDevelopmentSeeder
{
    private static readonly List<(Guid Id, string En, string Lt)> Fuels =
    [
        (Guid.Parse("11111111-0000-0000-0000-000000000001"), "Diesel", "Dyzelinas"),
        (Guid.Parse("11111111-0000-0000-0000-000000000002"), "Petrol", "Benzinas"),
        (Guid.Parse("11111111-0000-0000-0000-000000000003"), "Electric", "Elektra"),
        (Guid.Parse("11111111-0000-0000-0000-000000000004"), "Petrol/LPG", "Benzinas/Dujos"),
        (Guid.Parse("11111111-0000-0000-0000-000000000005"), "Petrol/Electric", "Benzinas/Elektra"),
        (Guid.Parse("11111111-0000-0000-0000-000000000006"), "Plug-in Hybrid", "Įkraunamas hibridas"),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Set<Fuel>().AnyAsync(cancellationToken))
            return;

        var entities = new List<Fuel>();
        foreach (var (id, en, lt) in Fuels)
        {
            entities.Add(new Fuel
            {
                Id = id,
                Name = en,
                Translations =
                [
                    new FuelTranslation { Id = Guid.NewGuid(), FuelId = id, LanguageCode = "en", Name = en },
                    new FuelTranslation { Id = Guid.NewGuid(), FuelId = id, LanguageCode = "lt", Name = lt },
                ]
            });
        }
        await context.AddRangeAsync(entities, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
