using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Infrastructure.Data.DatabaseContext;

public class AutomotiveContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Variant> Variants { get; set; }

    public DbSet<Drivetrain> Drivetrains { get; set; }

    public DbSet<Fuel> Fuels { get; set; }

    public DbSet<Transmission> Transmissions { get; set; }

    public DbSet<BodyType> BodyTypes { get; set; }

    public DbSet<FuelTranslation> FuelTranslations { get; set; }

    public DbSet<TransmissionTranslation> TransmissionTranslations { get; set; }

    public DbSet<BodyTypeTranslation> BodyTypeTranslations { get; set; }

    public DbSet<DrivetrainTranslation> DrivetrainTranslations { get; set; }

    public DbSet<Image> Images { get; set; }

    public DbSet<Listing> Listings { get; set; }

    public DbSet<User> Users { get; set; }

    public DbSet<UserPermission> UserPermissions { get; set; }

    public DbSet<Make> Makes { get; set; }

    public DbSet<Model> Models { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<Conversation> Conversations { get; set; }

    public DbSet<Message> Messages { get; set; }

    public DbSet<UserListingNote> UserListingNotes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VariantConfiguration).Assembly);
    }
}
