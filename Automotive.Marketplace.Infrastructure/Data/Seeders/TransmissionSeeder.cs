using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Infrastructure.Data.Seeders;

public class TransmissionSeeder(AutomotiveContext context) : ISeeder
{
    private static readonly List<(Guid Id, string En, string Lt)> Transmissions =
    [
        (Guid.Parse("22222222-0000-0000-0000-000000000001"), "Automatic", "Automatinis"),
        (Guid.Parse("22222222-0000-0000-0000-000000000002"), "Manual", "Mechaninis"),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Set<Transmission>().AnyAsync(cancellationToken))
            return;

        var entities = new List<Transmission>();
        foreach (var (id, en, lt) in Transmissions)
        {
            entities.Add(new Transmission
            {
                Id = id,
                Name = en,
                Translations =
                [
                    new TransmissionTranslation { Id = Guid.NewGuid(), TransmissionId = id, LanguageCode = "en", Name = en },
                    new TransmissionTranslation { Id = Guid.NewGuid(), TransmissionId = id, LanguageCode = "lt", Name = lt },
                ]
            });
        }
        await context.AddRangeAsync(entities, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
