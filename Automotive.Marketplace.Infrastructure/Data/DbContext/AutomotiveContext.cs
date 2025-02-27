namespace Automotive.Marketplace.Infrastructure.Data.DbContext
{
    using Automotive.Marketplace.Domain.Entities;
    using Microsoft.EntityFrameworkCore;

    public class AutomotiveContext : DbContext
    {
        public AutomotiveContext(DbContextOptions options)
        : base(options) { }

        DbSet<Make> Makes;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
        }

        public override int SaveChanges()
        {
            return base.SaveChanges();
        }
    }
}
