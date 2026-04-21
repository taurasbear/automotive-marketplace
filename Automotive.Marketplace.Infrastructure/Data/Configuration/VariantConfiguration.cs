using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class VariantConfiguration : IEntityTypeConfiguration<Variant>
{
    public void Configure(EntityTypeBuilder<Variant> builder)
    {
        builder.HasOne(v => v.Model)
            .WithMany(m => m.Variants)
            .HasForeignKey(v => v.ModelId);

        builder.HasOne(v => v.Fuel)
            .WithMany()
            .HasForeignKey(v => v.FuelId);

        builder.HasOne(v => v.Transmission)
            .WithMany()
            .HasForeignKey(v => v.TransmissionId);

        builder.HasOne(v => v.BodyType)
            .WithMany()
            .HasForeignKey(v => v.BodyTypeId);

        builder.HasIndex(v => new { v.ModelId, v.FuelId, v.TransmissionId, v.BodyTypeId })
               .IsUnique()
               .HasFilter("\"IsCustom\" = false");
    }
}
