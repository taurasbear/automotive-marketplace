using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class DefectCategoryTranslationConfiguration : IEntityTypeConfiguration<DefectCategoryTranslation>
{
    public void Configure(EntityTypeBuilder<DefectCategoryTranslation> builder)
    {
        builder.HasOne(t => t.DefectCategory)
            .WithMany(dc => dc.Translations)
            .HasForeignKey(t => t.DefectCategoryId);

        builder.HasIndex(t => new { t.DefectCategoryId, t.LanguageCode }).IsUnique();
    }
}