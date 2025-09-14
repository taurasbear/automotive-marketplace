using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasMany(user => user.LikedListings)
            .WithMany(listing => listing.LikeUsers)
            .UsingEntity<UserListingLike>(
                like => like.HasOne(like => like.Listing)
                .WithMany()
                .HasForeignKey(like => like.ListingId),
                like => like.HasOne(like => like.User)
                .WithMany()
                .HasForeignKey(like => like.UserId)
            );
    }
}
