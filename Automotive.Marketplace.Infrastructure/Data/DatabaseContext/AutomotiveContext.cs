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

    public DbSet<Offer> Offers { get; set; }

    public DbSet<UserListingNote> UserListingNotes { get; set; }

    public DbSet<Meeting> Meetings { get; set; }

    public DbSet<AvailabilityCard> AvailabilityCards { get; set; }

    public DbSet<AvailabilitySlot> AvailabilitySlots { get; set; }

    public DbSet<ContractCard> ContractCards { get; set; }

    public DbSet<ContractSellerSubmission> ContractSellerSubmissions { get; set; }

    public DbSet<ContractBuyerSubmission> ContractBuyerSubmissions { get; set; }

    public DbSet<DefectCategory> DefectCategories { get; set; }

    public DbSet<DefectCategoryTranslation> DefectCategoryTranslations { get; set; }

    public DbSet<ListingDefect> ListingDefects { get; set; }

    public DbSet<VehicleEfficiencyCache> VehicleEfficiencyCaches { get; set; }
    public DbSet<VehicleMarketCache> VehicleMarketCaches { get; set; }
    public DbSet<VehicleReliabilityCache> VehicleReliabilityCaches { get; set; }
    public DbSet<UserPreferences> UserPreferences { get; set; }
    public DbSet<ListingAiSummaryCache> ListingAiSummaryCaches { get; set; }

    public DbSet<Municipality> Municipalities { get; set; }

    public DbSet<MakeExclusion> MakeExclusions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VariantConfiguration).Assembly);
    }
}
