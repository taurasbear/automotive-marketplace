using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Infrastructure.Data.Seeders;

public class DefectCategorySeeder(AutomotiveContext context) : ISeeder
{
    private static readonly List<(Guid Id, string En, string Lt)> Categories =
    [
        (Guid.Parse("dd000001-0000-0000-0000-000000000001"), "Scratch", "Įbrėžimas"),
        (Guid.Parse("dd000001-0000-0000-0000-000000000002"), "Dent", "Įlenkimas"),
        (Guid.Parse("dd000001-0000-0000-0000-000000000003"), "Rust", "Rūdys"),
        (Guid.Parse("dd000001-0000-0000-0000-000000000004"), "Paint Damage", "Dažų pažeidimas"),
        (Guid.Parse("dd000001-0000-0000-0000-000000000005"), "Crack", "Įtrūkimas"),
        (Guid.Parse("dd000001-0000-0000-0000-000000000006"), "Corrosion", "Korozija"),
        (Guid.Parse("dd000001-0000-0000-0000-000000000007"), "Stain", "Dėmė"),
        (Guid.Parse("dd000001-0000-0000-0000-000000000008"), "Mechanical Wear", "Mechaninis nusidėvėjimas"),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Set<DefectCategory>().AnyAsync(cancellationToken))
            return;

        var entities = new List<DefectCategory>();
        foreach (var (id, en, lt) in Categories)
        {
            entities.Add(new DefectCategory
            {
                Id = id,
                Name = en,
                Translations =
                [
                    new DefectCategoryTranslation { Id = Guid.NewGuid(), DefectCategoryId = id, LanguageCode = "en", Name = en },
                    new DefectCategoryTranslation { Id = Guid.NewGuid(), DefectCategoryId = id, LanguageCode = "lt", Name = lt },
                ]
            });
        }
        await context.AddRangeAsync(entities, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}