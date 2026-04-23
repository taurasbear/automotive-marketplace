using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class AvailabilityCardConfiguration : IEntityTypeConfiguration<AvailabilityCard>
{
    public void Configure(EntityTypeBuilder<AvailabilityCard> builder)
    {
        builder.HasOne(a => a.Conversation)
            .WithMany()
            .HasForeignKey(a => a.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Initiator)
            .WithMany()
            .HasForeignKey(a => a.InitiatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Message)
            .WithOne(msg => msg.AvailabilityCard)
            .HasForeignKey<Message>(msg => msg.AvailabilityCardId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
