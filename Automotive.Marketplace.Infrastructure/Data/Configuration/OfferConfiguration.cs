using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.Property(o => o.Amount)
            .HasPrecision(18, 2);

        builder.HasOne(o => o.Conversation)
            .WithMany()
            .HasForeignKey(o => o.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.Initiator)
            .WithMany()
            .HasForeignKey(o => o.InitiatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.ParentOffer)
            .WithMany(o => o.CounterOffers)
            .HasForeignKey(o => o.ParentOfferId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Message)
            .WithOne(m => m.Offer)
            .HasForeignKey<Message>(m => m.OfferId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
