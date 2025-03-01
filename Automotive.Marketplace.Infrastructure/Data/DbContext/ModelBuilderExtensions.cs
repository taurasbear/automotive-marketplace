namespace Automotive.Marketplace.Infrastructure.Data.DbContext
{
    using Automotive.Marketplace.Domain.Entities;
    using Microsoft.EntityFrameworkCore;

    public static class ModelBuilderExtensions
    {
        public static void Seed(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Make>().HasData(
                new Make { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Toyota", CreatedAt = DateTime.UtcNow, CreatedBy = "System" },
                new Make { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "BMW", CreatedAt = DateTime.UtcNow, CreatedBy = "System" }
            );

            modelBuilder.Entity<Model>().HasData(
                new Model { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Camry", MakeId = Guid.Parse("11111111-1111-1111-1111-111111111111"), CreatedAt = DateTime.UtcNow, CreatedBy = "System" }
            );
        }
    }
}
