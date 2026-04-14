using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class BodyTypeTranslationConfiguration : IEntityTypeConfiguration<BodyTypeTranslation>
{
    public void Configure(EntityTypeBuilder<BodyTypeTranslation> builder)
    {
        builder.HasOne(t => t.BodyType)
            .WithMany(b => b.Translations)
            .HasForeignKey(t => t.BodyTypeId);

        builder.HasIndex(t => new { t.BodyTypeId, t.LanguageCode }).IsUnique();
    }
}
