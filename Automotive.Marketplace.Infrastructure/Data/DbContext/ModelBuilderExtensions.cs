namespace Automotive.Marketplace.Infrastructure.Data.DbContext;

using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Microsoft.EntityFrameworkCore;

public static class ModelBuilderExtensions
{
    public static void Seed(this ModelBuilder modelBuilder)
    {
        var today = new DateTime(2025, 4, 3, 19, 46, 19, DateTimeKind.Utc);
        modelBuilder.Entity<Make>().HasData(
            new Make
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Toyota",
                CreatedAt = today,
                CreatedBy = "System"
            },
            new Make
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "BMW",
                CreatedAt = today,
                CreatedBy = "System"
            }
        );

        modelBuilder.Entity<Model>().HasData(
            new Model
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = "Camry",
                MakeId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CreatedAt = today,
                CreatedBy = "System"
            }
        );

        modelBuilder.Entity<Car>().HasData(
            new Car
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Year = new DateTime(2002, 4, 13, 0, 0, 0, DateTimeKind.Utc),
                ModelId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Fuel = Fuel.Diesel,
                Drivetrain = Drivetrain.RWD
            }
        );

        modelBuilder.Entity<CarDetails>().HasData(
            new CarDetails
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                CarId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Mileage = 26700,
                Power = 97,
                EngineSize = 1300,
                Used = true
            },
            new CarDetails
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                CarId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Mileage = 200000,
                Power = 102,
                EngineSize = 1400,
                Used = false
            }
        );

        modelBuilder.Entity<User>().HasData(
            new User { Id = Guid.Parse("99999999-9999-9999-9999-999999999999"), Username = "Ben" },
            new User
            {
                Id = Guid.Parse("0198e34c-81ad-7498-9828-5d8c530a994a"),
                Username = "taurasbear",
                Email = "bear@gmail.com",
                HashedPassword = "$2a$11$0gIGetjT4PZ8bfGrXUrwoOImxqNeLM9m.0NR9EEu2mc1UcJocRxQ6",
            }
        );

        modelBuilder.Entity<UserPermission>().HasData(
            new UserPermission
            {
                Id = Guid.Parse("99999999-9299-9999-9999-999999999999"),
                Permission = Permission.ViewListings,
                UserId = Guid.Parse("0198e34c-81ad-7498-9828-5d8c530a994a")
            }
        );

        modelBuilder.Entity<Listing>().HasData(
            new Listing
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                CarDetailsId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                SellerId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                City = "Kaunas",
                Description = "Smulkūs kėbulo defektai",
                Price = 800,
                Status = Status.Available
            },
            new Listing
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                CarDetailsId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                SellerId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                City = "Vilnius",
                Description = "Be defektu",
                Price = 130,
                Status = Status.Available
            }
        );
    }
}
