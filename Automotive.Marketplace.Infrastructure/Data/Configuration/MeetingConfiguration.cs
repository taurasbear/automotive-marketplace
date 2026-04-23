using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class MeetingConfiguration : IEntityTypeConfiguration<Meeting>
{
    public void Configure(EntityTypeBuilder<Meeting> builder)
    {
        builder.Property(m => m.LocationLat)
            .HasPrecision(10, 7);

        builder.Property(m => m.LocationLng)
            .HasPrecision(10, 7);

        builder.HasOne(m => m.Conversation)
            .WithMany()
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Initiator)
            .WithMany()
            .HasForeignKey(m => m.InitiatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.ParentMeeting)
            .WithMany(m => m.CounterMeetings)
            .HasForeignKey(m => m.ParentMeetingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Message)
            .WithOne(msg => msg.Meeting)
            .HasForeignKey<Message>(msg => msg.MeetingId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
