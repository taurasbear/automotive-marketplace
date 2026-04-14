using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class ListingConfiguration : IEntityTypeConfiguration<Listing>
{
    public void Configure(EntityTypeBuilder<Listing> builder)
    {
        builder.HasOne(listing => listing.Variant)
            .WithMany(variant => variant.Listings)
            .HasForeignKey(listing => listing.VariantId);

        builder.HasOne(listing => listing.Drivetrain)
            .WithMany()
            .HasForeignKey(listing => listing.DrivetrainId);

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
