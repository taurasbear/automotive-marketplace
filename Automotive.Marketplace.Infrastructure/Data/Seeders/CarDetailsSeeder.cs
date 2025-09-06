using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Automotive.Marketplace.Infrastructure.Data.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Seeders;

public class CarDetailsSeeder(AutomotiveContext context) : IDevelopmentSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Set<CarDetails>()
            .AnyAsync(cancellationToken))
        {
            return;
        }

        var cars = await context.Set<Car>()
            .ToListAsync(cancellationToken);

        foreach (var car in cars)
        {
            var carDetails = new CarDetailsBuilder()
                .WithCar(car.Id)
                .Build();

            await context.AddAsync(carDetails, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}