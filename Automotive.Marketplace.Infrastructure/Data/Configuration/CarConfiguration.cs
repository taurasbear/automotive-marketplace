using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class CarConfiguration : IEntityTypeConfiguration<Car>
{
    public void Configure(EntityTypeBuilder<Car> builder)
    {
        builder.HasOne(car => car.Model)
            .WithMany(model => model.Cars)
            .HasForeignKey(car => car.ModelId);

        builder.Property(car => car.Transmission)
            .HasConversion(
                transmissionEnum => transmissionEnum.ToString(),
                transmissionString => (Transmission)Enum.Parse(typeof(Transmission), transmissionString)
            );

        builder.Property(car => car.Fuel)
            .HasConversion(
                fuelEnum => fuelEnum.ToString(),
                fuelString => (Fuel)Enum.Parse(typeof(Fuel), fuelString)
            );

        builder.Property(car => car.BodyType)
            .HasConversion(
                bodyTypeEnum => bodyTypeEnum.ToString(),
                bodyTypeString => (BodyType)Enum.Parse(typeof(BodyType), bodyTypeString)
            );

        builder.Property(car => car.Drivetrain)
            .HasConversion(
                drivetrainEnum => drivetrainEnum.ToString(),
                drivetrainString => (Drivetrain)Enum.Parse(typeof(Drivetrain), drivetrainString)
            );
    }
}
