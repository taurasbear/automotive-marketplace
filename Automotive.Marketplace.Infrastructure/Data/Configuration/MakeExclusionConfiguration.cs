using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class MakeExclusionConfiguration : IEntityTypeConfiguration<MakeExclusion>
{
    public void Configure(EntityTypeBuilder<MakeExclusion> builder)
    {
        builder.HasKey(e => e.VpicId);
        builder.Property(e => e.VpicId).ValueGeneratedNever();
        builder.Property(e => e.VpicName).IsRequired().HasMaxLength(200);
    }
}
