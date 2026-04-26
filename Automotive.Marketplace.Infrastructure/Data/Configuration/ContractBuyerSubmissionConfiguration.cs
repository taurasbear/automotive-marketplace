using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class ContractBuyerSubmissionConfiguration : IEntityTypeConfiguration<ContractBuyerSubmission>
{
    public void Configure(EntityTypeBuilder<ContractBuyerSubmission> builder)
    {
        builder.Property(b => b.PersonalIdCode).HasMaxLength(50);
        builder.Property(b => b.FullName).HasMaxLength(200);
        builder.Property(b => b.Phone).HasMaxLength(30);
        builder.Property(b => b.Email).HasMaxLength(200);
        builder.Property(b => b.Address).HasMaxLength(500);
    }
}
