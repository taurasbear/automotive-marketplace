using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class ListingAiSummaryCacheConfiguration : IEntityTypeConfiguration<ListingAiSummaryCache>
{
    public void Configure(EntityTypeBuilder<ListingAiSummaryCache> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.SummaryType).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Summary).IsRequired();
        builder.HasIndex(e => new { e.ListingId, e.SummaryType, e.ComparisonListingId }).IsUnique();
    }
}
