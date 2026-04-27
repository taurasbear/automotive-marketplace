using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class ContractSellerSubmissionConfiguration : IEntityTypeConfiguration<ContractSellerSubmission>
{
    public void Configure(EntityTypeBuilder<ContractSellerSubmission> builder)
    {
        builder.Property(s => s.Price).HasPrecision(18, 2);
        builder.Property(s => s.Make).HasMaxLength(100);
        builder.Property(s => s.CommercialName).HasMaxLength(200);
        builder.Property(s => s.RegistrationNumber).HasMaxLength(20);
        builder.Property(s => s.PersonalIdCode).HasMaxLength(50);
        builder.Property(s => s.FullName).HasMaxLength(200);
        builder.Property(s => s.Phone).HasMaxLength(30);
        builder.Property(s => s.Email).HasMaxLength(200);
        builder.Property(s => s.Address).HasMaxLength(500);
        builder.Property(s => s.Country).HasMaxLength(100);
    }
}
