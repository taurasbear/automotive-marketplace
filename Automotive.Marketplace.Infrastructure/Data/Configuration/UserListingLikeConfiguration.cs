using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class UserListingLikeConfiguration : IEntityTypeConfiguration<UserListingLike>
{
    public void Configure(EntityTypeBuilder<UserListingLike> builder)
    {
        builder.HasIndex(like => new { like.UserId, like.ListingId })
            .IsUnique();
    }
}
