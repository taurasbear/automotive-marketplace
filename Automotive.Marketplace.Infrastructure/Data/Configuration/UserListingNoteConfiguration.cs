using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Automotive.Marketplace.Infrastructure.Data.Configuration;

public class UserListingNoteConfiguration : IEntityTypeConfiguration<UserListingNote>
{
    public void Configure(EntityTypeBuilder<UserListingNote> builder)
    {
        builder.HasIndex(note => new { note.UserId, note.ListingId })
            .IsUnique();

        builder.HasOne(note => note.Listing)
            .WithMany()
            .HasForeignKey(note => note.ListingId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(note => note.User)
            .WithMany()
            .HasForeignKey(note => note.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
