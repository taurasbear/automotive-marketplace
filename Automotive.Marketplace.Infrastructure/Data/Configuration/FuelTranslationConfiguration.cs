using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class FuelTranslationConfiguration : IEntityTypeConfiguration<FuelTranslation>
{
    public void Configure(EntityTypeBuilder<FuelTranslation> builder)
    {
        builder.HasOne(t => t.Fuel)
            .WithMany(f => f.Translations)
            .HasForeignKey(t => t.FuelId);

        builder.HasIndex(t => new { t.FuelId, t.LanguageCode }).IsUnique();
    }
}
