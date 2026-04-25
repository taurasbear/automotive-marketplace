using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class VehicleMarketCacheConfiguration : IEntityTypeConfiguration<VehicleMarketCache>
{
    public void Configure(EntityTypeBuilder<VehicleMarketCache> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Make).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Model).HasMaxLength(100).IsRequired();
        builder.Property(e => e.MedianPrice).HasColumnType("decimal(18,2)");
        builder.HasIndex(e => new { e.Make, e.Model, e.Year }).IsUnique();
    }
}
