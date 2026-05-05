using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Infrastructure.Data.Seeders;

public class ListingSeeder(AutomotiveContext context) : IDevelopmentSeeder
{
    private const int ListingsPerVariant = 3;
    private const string ImageBucket = "automotive-marketplace";
    private const string ImageContentType = "image/jpeg";

    // Object keys from MinIO — these always exist and are reused across all seeded listings
    private static readonly (string ObjectKey, long FileSize)[] ImagePool =
    [
        ("990cc19f-6170-4040-bd4a-5969ee9d49a9_blob", 219264),
        ("a67034b0-e181-4621-81e9-69c65699f558_blob", 109360),
        ("9f164586-075d-4175-8d6b-6bec864c4f7e_blob", 112069),
        ("538a5945-87cc-4b06-b883-6c4752c3c6eb_blob", 130509),
        ("db32da51-1fd9-40ef-b3ff-602cfbb8cee1_blob", 120543),
        ("43c288fa-1c33-472a-a685-906b7081ca8f_blob", 136368),
        ("2177eb28-8c50-425e-98ae-5d6d9ca1af31_blob",  89596),
        ("d499f2e6-16a3-49af-a510-3f2a69a5ace2_blob",  76873),
        ("bfbaa126-dcb1-4f6a-a8f1-1fa29118d7a0_blob",  79790),
        ("ae7e6f6c-af25-4a73-8da7-1fe3696a2c3f_blob",  81381),
        ("3afaea84-8ba4-4016-b4d4-c718e7d90a5c_blob",  82002),
        ("c90adf9d-a832-4e97-9046-e132cda0779e_blob",  84521),
        ("0d2457e9-701c-41f8-a391-3eb74935794b_blob",  90066),
        ("69b5a73e-3b86-4849-b808-dd31f6cfa3b1_blob",  60872),
        ("b0c7d74b-3bdf-406a-8ca0-ecf743a10f3b_blob", 235123),
        ("8060f142-baca-4744-9685-13e2f7fb2898_blob", 134671),
        ("84a67337-0b27-4de2-a27a-7cf72559b302_blob", 110001),
        ("722161c8-6947-40c3-bb86-644f5e204cf9_blob",  65970),
        ("06bdc8b0-70cd-4e67-b337-988e85d1ad43_blob", 151376),
        ("196e2e2d-ad6c-4cae-b32a-5c96605b1aa3_blob", 118278),
        ("e3f5fbb9-182e-43bf-84b0-b0b7971332a1_blob", 106844),
        ("07c6dba5-7764-45ae-9c94-cc4797e622d8_blob", 109595),
        ("0ed02000-b264-417a-b20f-1b2e67de509c_blob", 103822),
    ];

    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Set<Listing>().AnyAsync(cancellationToken))
            return;

        var users = await context.Set<User>().ToListAsync(cancellationToken);
        var variants = await context.Set<Variant>()
            .Include(v => v.Model).ThenInclude(m => m.Make)
            .ToListAsync(cancellationToken);
        var drivetrains = await context.Set<Drivetrain>().ToListAsync(cancellationToken);
        var municipalities = await context.Set<Municipality>().ToListAsync(cancellationToken);

        if (!users.Any() || !variants.Any() || !drivetrains.Any() || !municipalities.Any())
            return;

        var fwd = drivetrains.First(d => d.Name == "FWD");
        var rwd = drivetrains.First(d => d.Name == "RWD");
        var awd = drivetrains.First(d => d.Name == "AWD");

        var random = new Random(42);
        var currentYear = DateTime.Now.Year;
        var listingIndex = 0;

        foreach (var variant in variants)
        {
            var makeName = variant.Model.Make.Name;
            var drivetrain = makeName == "Subaru" ? awd
                : variant.PowerKw >= 220 ? rwd
                : fwd;

            for (var i = 0; i < ListingsPerVariant; i++)
            {
                var year = random.Next(2008, currentYear + 1);
                var age = currentYear - year;
                var mileage = Math.Clamp(
                    age * random.Next(8000, 22000) + random.Next(-3000, 3000),
                    min: 2000, max: 380000);
                var basePrice = (year - 2004) * 700 + variant.PowerKw * 25;
                var priceMultiplier = (decimal)(0.8 + random.NextDouble() * 0.4);
                var price = Math.Max((decimal)basePrice * priceMultiplier, 800m);

                var user = users[listingIndex % users.Count];
                var municipality = municipalities[listingIndex % municipalities.Count];

                var listing = new ListingBuilder()
                    .WithVariant(variant.Id)
                    .WithSeller(user.Id)
                    .WithDrivetrain(drivetrain.Id)
                    .WithMunicipality(municipality.Id)
                    .WithYear(year)
                    .WithMileage(mileage)
                    .WithPrice(price)
                    .Build();

                await context.AddAsync(listing, cancellationToken);

                var imageCount = random.Next(2, 6);
                var shuffled = ImagePool.OrderBy(_ => random.Next()).Take(imageCount);
                foreach (var (objectKey, fileSize) in shuffled)
                {
                    await context.AddAsync(new Image
                    {
                        Id               = Guid.NewGuid(),
                        ListingId        = listing.Id,
                        BucketName       = ImageBucket,
                        ObjectKey        = objectKey,
                        OriginalFileName = "blob",
                        ContentType      = ImageContentType,
                        FileSize         = fileSize,
                        AltText          = listing.Id.ToString(),
                        CreatedAt        = DateTime.UtcNow,
                        CreatedBy        = "seeder",
                        ModifiedBy       = "seeder",
                    }, cancellationToken);
                }

                listingIndex++;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}