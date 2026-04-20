using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class DrivetrainConfiguration : IEntityTypeConfiguration<Drivetrain>
{
    public void Configure(EntityTypeBuilder<Drivetrain> builder)
    {
        builder.HasMany(d => d.Translations)
            .WithOne(t => t.Drivetrain)
            .HasForeignKey(t => t.DrivetrainId);
    }
}
