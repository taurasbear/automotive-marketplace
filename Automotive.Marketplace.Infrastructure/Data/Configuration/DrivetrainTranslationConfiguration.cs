using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class DrivetrainTranslationConfiguration : IEntityTypeConfiguration<DrivetrainTranslation>
{
    public void Configure(EntityTypeBuilder<DrivetrainTranslation> builder)
    {
        builder.HasOne(t => t.Drivetrain)
            .WithMany(d => d.Translations)
            .HasForeignKey(t => t.DrivetrainId);

        builder.HasIndex(t => new { t.DrivetrainId, t.LanguageCode }).IsUnique();
    }
}
