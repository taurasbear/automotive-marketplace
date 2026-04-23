using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasOne(m => m.Conversation)
            .WithMany(c => c.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(m => m.MessageType)
            .HasDefaultValue(MessageType.Text)
            .HasSentinel(MessageType.Text);

        builder.Property(m => m.OfferId)
            .IsRequired(false);

        builder.Property(m => m.MeetingId)
            .IsRequired(false);

        builder.Property(m => m.AvailabilityCardId)
            .IsRequired(false);
    }
}
