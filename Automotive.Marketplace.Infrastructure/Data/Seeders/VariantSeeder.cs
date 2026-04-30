using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Infrastructure.Data.Seeders;

public class VariantSeeder(AutomotiveContext context) : ISeeder
{
    // IDs match FuelSeeder, TransmissionSeeder, and BodyTypeSeeder
    private static readonly Guid Petrol    = Guid.Parse("11111111-0000-0000-0000-000000000002");
    private static readonly Guid Diesel    = Guid.Parse("11111111-0000-0000-0000-000000000001");
    private static readonly Guid Electric  = Guid.Parse("11111111-0000-0000-0000-000000000003");
    private static readonly Guid HybridPE  = Guid.Parse("11111111-0000-0000-0000-000000000005");

    private static readonly Guid Auto   = Guid.Parse("22222222-0000-0000-0000-000000000001");
    private static readonly Guid Manual  = Guid.Parse("22222222-0000-0000-0000-000000000002");

    private static readonly Guid Sedan     = Guid.Parse("33333333-0000-0000-0000-000000000001");
    private static readonly Guid Hatchback = Guid.Parse("33333333-0000-0000-0000-000000000002");
    private static readonly Guid Wagon     = Guid.Parse("33333333-0000-0000-0000-000000000003");
    private static readonly Guid Coupe     = Guid.Parse("33333333-0000-0000-0000-000000000004");
    private static readonly Guid Suv       = Guid.Parse("33333333-0000-0000-0000-000000000005");

    private record VariantSpec(
        string Make, string Model,
        int EngineSizeMl, int PowerKw,
        Guid FuelId, Guid TransmissionId, Guid BodyTypeId, int DoorCount);

    private static readonly List<VariantSpec> Specs =
    [
        // Honda
        new("Honda", "Civic",        1500,  95,  Petrol,   Manual, Hatchback, 5),
        new("Honda", "Civic",        1500, 125,  Petrol,   Auto,   Sedan,     4),
        new("Honda", "Civic Type R", 2000, 228,  Petrol,   Manual, Hatchback, 5),
        new("Honda", "Accord",       2000, 145,  Petrol,   Auto,   Sedan,     4),

        // Mazda
        new("Mazda", "Mazda6",  2000, 121,  Petrol, Manual, Sedan,     4),
        new("Mazda", "Mazda6",  2200, 110,  Diesel, Auto,   Wagon,     5),
        new("Mazda", "Mazda3",  2000, 121,  Petrol, Manual, Hatchback, 5),
        new("Mazda", "Mx-5",    2000, 135,  Petrol, Manual, Coupe,     2),

        // Subaru
        new("Subaru", "Impreza",  2000, 110,  Petrol, Manual, Hatchback, 5),
        new("Subaru", "Forester", 2500, 136,  Petrol, Auto,   Suv,       5),
        new("Subaru", "Outback",  2500, 136,  Petrol, Auto,   Wagon,     5),
        new("Subaru", "Legacy",   2500, 123,  Petrol, Manual, Sedan,     4),
        new("Subaru", "Wrx",      2000, 221,  Petrol, Manual, Sedan,     4),

        // Kia
        new("Kia", "Rio",      1400,  74,  Petrol, Manual, Hatchback, 5),
        new("Kia", "Sportage", 1600,  97,  Diesel, Auto,   Suv,       5),
        new("Kia", "Forte",    2000, 112,  Petrol, Auto,   Sedan,     4),
        new("Kia", "Stinger",  3300, 272,  Petrol, Auto,   Sedan,     4),

        // Toyota
        new("Toyota", "Corolla", 1800, 103,  Petrol,  Manual, Hatchback, 5),
        new("Toyota", "Corolla", 1800,  90,  HybridPE, Auto,  Sedan,     4),
        new("Toyota", "Camry",   2500, 134,  Petrol,   Auto,  Sedan,     4),
        new("Toyota", "Yaris",   1000,  72,  Petrol,  Manual, Hatchback, 5),

        // Volkswagen
        new("Volkswagen", "Golf",   1500, 110,  Petrol, Auto,   Hatchback, 5),
        new("Volkswagen", "Golf",   2000, 110,  Diesel, Manual, Hatchback, 5),
        new("Volkswagen", "Passat", 2000, 110,  Diesel, Auto,   Sedan,     4),

        // Ford
        new("Ford", "Focus",  1000,  74,  Petrol, Manual, Hatchback, 5),
        new("Ford", "Focus",  1500,  88,  Diesel, Manual, Wagon,     5),
        new("Ford", "Fiesta", 1000,  74,  Petrol, Manual, Hatchback, 5),

        // Hyundai
        new("Hyundai", "Elantra", 1600,  97,  Petrol, Auto, Sedan, 4),
        new("Hyundai", "Tucson",  2000, 115,  Diesel, Auto, Suv,   5),

        // Nissan
        new("Nissan", "Micra", 1200,  65,  Petrol,   Manual, Hatchback, 5),
        new("Nissan", "Leaf",     0, 110,  Electric, Auto,   Hatchback, 5),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Set<Variant>().AnyAsync(cancellationToken))
            return;

        var targetMakes = Specs.Select(s => s.Make).ToHashSet();
        var allModels = await context.Set<Model>()
            .Include(m => m.Make)
            .Where(m => targetMakes.Contains(m.Make.Name))
            .ToListAsync(cancellationToken);

        var modelsByKey = allModels
            .GroupBy(m => (m.Make.Name, m.Name))
            .ToDictionary(g => g.Key, g => g.First());

        foreach (var spec in Specs)
        {
            if (!modelsByKey.TryGetValue((spec.Make, spec.Model), out var model))
                continue;

            context.Add(new Variant
            {
                Id             = Guid.NewGuid(),
                ModelId        = model.Id,
                EngineSizeMl   = spec.EngineSizeMl,
                PowerKw        = spec.PowerKw,
                FuelId         = spec.FuelId,
                TransmissionId = spec.TransmissionId,
                BodyTypeId     = spec.BodyTypeId,
                DoorCount      = spec.DoorCount,
                IsCustom       = false,
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
