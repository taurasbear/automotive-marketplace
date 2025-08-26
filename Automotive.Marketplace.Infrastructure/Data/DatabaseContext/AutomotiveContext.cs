using Automotive.Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Infrastructure.Data.DatabaseContext;

public class AutomotiveContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Car> Cars { get; set; }

    public DbSet<CarDetails> CarsDetails { get; set; }

    public DbSet<Image> Images { get; set; }

    public DbSet<Listing> Listings { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<UserPermission> UserPermissions { get; set; }

    public DbSet<Make> Makes { get; set; }

    public DbSet<Model> Models { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Model>()
            .HasOne(model => model.Make)
            .WithMany(make => make.Models)
            .HasForeignKey(model => model.MakeId);

        modelBuilder.Entity<Car>()
            .HasOne(car => car.Model)
            .WithMany(model => model.Cars)
            .HasForeignKey(car => car.ModelId);

        modelBuilder.Entity<CarDetails>()
            .HasOne(carDetails => carDetails.Car)
            .WithMany(car => car.CarDetails)
            .HasForeignKey(carDetails => carDetails.CarId);

        modelBuilder.Entity<Listing>()
            .HasOne(listing => listing.CarDetails)
            .WithOne(carDetails => carDetails.Listing)
            .HasForeignKey<Listing>(listing => listing.CarDetailsId);

        modelBuilder.Entity<Image>()
            .HasOne(image => image.Listing)
            .WithMany(listing => listing.Images)
            .HasForeignKey(image => image.ListingId);

        modelBuilder.Entity<Listing>()
            .HasOne(listing => listing.Seller)
            .WithMany(user => user.Listings)
            .HasForeignKey(listing => listing.SellerId);

        modelBuilder.Entity<User>()
            .HasMany(user => user.LikedListings)
            .WithMany(listing => listing.LikeUsers)
            .UsingEntity<UserListingLike>(
                like => like.HasOne(like => like.Listing)
                .WithMany()
                .HasForeignKey(like => like.ListingId),
                like => like.HasOne(like => like.User)
                .WithMany()
                .HasForeignKey(like => like.UserId)
            );

        modelBuilder.Entity<UserListingLike>()
            .HasIndex(like => new { like.UserId, like.ListingId })
            .IsUnique();

        modelBuilder.Entity<RefreshToken>()
            .HasOne(refreshToken => refreshToken.User)
            .WithMany()
            .HasForeignKey(refreshToken => refreshToken.UserId);

        modelBuilder.Entity<UserPermission>()
            .HasOne(userPermission => userPermission.User)
            .WithMany(user => user.UserPermissions)
            .HasForeignKey(userPermission => userPermission.UserId);

        modelBuilder.Seed();
    }

    public override int SaveChanges()
    {
        //var entries = ChangeTracker.Entries().Where()
        return base.SaveChanges();
    }
}
