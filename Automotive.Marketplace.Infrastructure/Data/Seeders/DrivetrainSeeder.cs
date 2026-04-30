using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Infrastructure.Data.Seeders;

public class DrivetrainSeeder(AutomotiveContext context) : ISeeder
{
    private static readonly List<(Guid Id, string En, string Lt)> Drivetrains =
    [
        (Guid.Parse("44444444-0000-0000-0000-000000000001"), "FWD", "Priekinis pavara"),
        (Guid.Parse("44444444-0000-0000-0000-000000000002"), "RWD", "Galinė pavara"),
        (Guid.Parse("44444444-0000-0000-0000-000000000003"), "AWD", "Visų ratų pavara"),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Set<Drivetrain>().AnyAsync(cancellationToken))
            return;

        var entities = new List<Drivetrain>();
        foreach (var (id, en, lt) in Drivetrains)
        {
            entities.Add(new Drivetrain
            {
                Id = id,
                Name = en,
                Translations =
                [
                    new DrivetrainTranslation { Id = Guid.NewGuid(), DrivetrainId = id, LanguageCode = "en", Name = en },
                    new DrivetrainTranslation { Id = Guid.NewGuid(), DrivetrainId = id, LanguageCode = "lt", Name = lt },
                ]
            });
        }
        await context.AddRangeAsync(entities, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
