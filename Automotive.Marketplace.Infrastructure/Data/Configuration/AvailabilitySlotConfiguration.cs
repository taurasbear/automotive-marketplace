using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class AvailabilitySlotConfiguration : IEntityTypeConfiguration<AvailabilitySlot>
{
    public void Configure(EntityTypeBuilder<AvailabilitySlot> builder)
    {
        builder.HasOne(s => s.AvailabilityCard)
            .WithMany(a => a.Slots)
            .HasForeignKey(s => s.AvailabilityCardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
