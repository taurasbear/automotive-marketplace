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

        public DbSet<Login> Logins { get; set; }

        public DbSet<Make> Makes { get; set; }

        public DbSet<Model> Models { get; set; }

        public DbSet<Seller> Sellers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }

        public override int SaveChanges()
        {
            //var entries = ChangeTracker.Entries().Where()
            return base.SaveChanges();
        }
    }
}
