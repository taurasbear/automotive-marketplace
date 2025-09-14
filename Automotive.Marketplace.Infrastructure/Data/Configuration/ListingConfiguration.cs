using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.HasOne(listing => listing.Car)
            .WithMany(car => car.Listings)
            .HasForeignKey(listing => listing.CarId);

        builder.HasOne(listing => listing.Seller)
            .WithMany(user => user.Listings)
            .HasForeignKey(listing => listing.SellerId);

        builder.Property(listing => listing.Status)
            .HasConversion(
                statusEnum => statusEnum.ToString(),
                statusString => (Status)Enum.Parse(typeof(Status), statusString)
            );
    }
}
