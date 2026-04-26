using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class ContractCardConfiguration : IEntityTypeConfiguration<ContractCard>
{
    public void Configure(EntityTypeBuilder<ContractCard> builder)
    {
        builder.HasOne(c => c.Conversation)
            .WithMany()
            .HasForeignKey(c => c.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Initiator)
            .WithMany()
            .HasForeignKey(c => c.InitiatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Message)
            .WithOne(m => m.ContractCard)
            .HasForeignKey<Message>(m => m.ContractCardId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.SellerSubmission)
            .WithOne(s => s.ContractCard)
            .HasForeignKey<ContractSellerSubmission>(s => s.ContractCardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.BuyerSubmission)
            .WithOne(b => b.ContractCard)
            .HasForeignKey<ContractBuyerSubmission>(b => b.ContractCardId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
