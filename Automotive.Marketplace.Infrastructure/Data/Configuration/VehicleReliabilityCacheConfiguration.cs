using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class VehicleReliabilityCacheConfiguration : IEntityTypeConfiguration<VehicleReliabilityCache>
{
    public void Configure(EntityTypeBuilder<VehicleReliabilityCache> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Make).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Model).HasMaxLength(100).IsRequired();
        builder.HasIndex(e => new { e.Make, e.Model, e.Year }).IsUnique();
    }
}
