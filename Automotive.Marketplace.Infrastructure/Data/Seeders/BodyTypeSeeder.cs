using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Infrastructure.Data.Seeders;

public class BodyTypeSeeder(AutomotiveContext context) : IDevelopmentSeeder
{
    private static readonly List<(Guid Id, string En, string Lt)> BodyTypes =
    [
        (Guid.Parse("33333333-0000-0000-0000-000000000001"), "Sedan", "Sedanas"),
        (Guid.Parse("33333333-0000-0000-0000-000000000002"), "Hatchback", "Hečbekas"),
        (Guid.Parse("33333333-0000-0000-0000-000000000003"), "Wagon", "Universalas"),
        (Guid.Parse("33333333-0000-0000-0000-000000000004"), "Coupe", "Kupė"),
        (Guid.Parse("33333333-0000-0000-0000-000000000005"), "SUV", "Visureigis"),
        (Guid.Parse("33333333-0000-0000-0000-000000000006"), "Minivan", "Minivenas"),
        (Guid.Parse("33333333-0000-0000-0000-000000000007"), "Crossover", "Krosoveris"),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Set<BodyType>().AnyAsync(cancellationToken))
            return;

        var entities = new List<BodyType>();
        foreach (var (id, en, lt) in BodyTypes)
        {
            entities.Add(new BodyType
            {
                Id = id,
                Name = en,
                Translations =
                [
                    new BodyTypeTranslation { Id = Guid.NewGuid(), BodyTypeId = id, LanguageCode = "en", Name = en },
                    new BodyTypeTranslation { Id = Guid.NewGuid(), BodyTypeId = id, LanguageCode = "lt", Name = lt },
                ]
            });
        }
        await context.AddRangeAsync(entities, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
