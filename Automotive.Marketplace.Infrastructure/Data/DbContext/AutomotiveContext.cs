namespace Automotive.Marketplace.Infrastructure.Data.DbContext
{
    using Automotive.Marketplace.Domain.Entities;
    using Microsoft.EntityFrameworkCore;

    public class AutomotiveContext : DbContext
    {
        public AutomotiveContext(DbContextOptions options)
        : base(options) { }

        public DbSet<Admin> Admins { get; set; }

        public DbSet<Car> Cars { get; set; }

        public DbSet<CarDetails> CarsDetails { get; set; }

        public DbSet<Client> Clients { get; set; }

        public DbSet<Image> Images { get; set; }

        public DbSet<Listing> Listings { get; set; }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<Make> Makes { get; set; }

        public DbSet<Model> Models { get; set; }

        public DbSet<Seller> Sellers { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Model>()
                .HasOne(mo => mo.Make)
                .WithMany(ma => ma.Models)
                .HasForeignKey(mo => mo.MakeId);

            modelBuilder.Entity<Car>()
                .HasOne(c => c.Model)
                .WithMany(md => md.Cars)
                .HasForeignKey(c => c.ModelId);

            modelBuilder.Entity<CarDetails>()
                .HasOne(cd => cd.Car)
                .WithMany(c => c.CarDetails)
                .HasForeignKey(cd => cd.CarId);

            modelBuilder.Entity<Listing>()
                .HasOne(l => l.CarDetails)
                .WithOne(cd => cd.Listing)
                .HasForeignKey<Listing>(l => l.CarDetailsId);

            modelBuilder.Entity<Image>()
                .HasOne(i => i.Listing)
                .WithMany(l => l.Images)
                .HasForeignKey(i => i.ListingId);

            modelBuilder.Entity<Listing>()
                .HasOne(l => l.Seller)
                .WithMany(s => s.Listings)
                .HasForeignKey(l => l.SellerId);

            modelBuilder.Entity<Client>()
                .HasMany(c => c.LikedListings)
                .WithMany(l => l.LikeClients)
                .UsingEntity<ClientListingLike>(
                    cll => cll.HasOne(cll => cll.Listing)
                    .WithMany()
                    .HasForeignKey(cll => cll.ListingId),
                    cll => cll.HasOne(cll => cll.Client)
                    .WithMany()
                    .HasForeignKey(cll => cll.ClientId)
                );

            modelBuilder.Entity<ClientListingLike>()
                .HasIndex(cll => new { cll.ClientId, cll.ListingId })
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.Account)
                .WithMany()
                .HasForeignKey(rt => rt.AccountId);

            modelBuilder.Seed();
        }

        public override int SaveChanges()
        {
            //var entries = ChangeTracker.Entries().Where()
            return base.SaveChanges();
        }
    }
}
