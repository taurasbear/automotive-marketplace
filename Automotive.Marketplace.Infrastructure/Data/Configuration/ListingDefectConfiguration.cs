using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class ListingDefectConfiguration : IEntityTypeConfiguration<ListingDefect>
{
    public void Configure(EntityTypeBuilder<ListingDefect> builder)
    {
        builder.HasOne(ld => ld.Listing)
            .WithMany(l => l.Defects)
            .HasForeignKey(ld => ld.ListingId);

        builder.HasOne(ld => ld.DefectCategory)
            .WithMany()
            .HasForeignKey(ld => ld.DefectCategoryId)
            .IsRequired(false);
    }
}