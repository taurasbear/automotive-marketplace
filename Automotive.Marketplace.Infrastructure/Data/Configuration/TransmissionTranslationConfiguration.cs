using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class TransmissionTranslationConfiguration : IEntityTypeConfiguration<TransmissionTranslation>
{
    public void Configure(EntityTypeBuilder<TransmissionTranslation> builder)
    {
        builder.HasOne(t => t.Transmission)
            .WithMany(tr => tr.Translations)
            .HasForeignKey(t => t.TransmissionId);

        builder.HasIndex(t => new { t.TransmissionId, t.LanguageCode }).IsUnique();
    }
}
