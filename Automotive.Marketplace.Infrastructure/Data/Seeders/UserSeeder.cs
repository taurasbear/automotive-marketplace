using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Infrastructure.Data.Seeders;

public class UserSeeder(AutomotiveContext context, IPasswordHasher passwordHasher) : IDevelopmentSeeder
{
    public async Task SeedAsync(CancellationToken cancellationToken)
    {
        if (await context.Set<User>()
            .AnyAsync(cancellationToken))
        {
            return;
        }

        var superuser = CreateSuperuser();
        var regularUser = CreateRegularUser();

        await context.AddRangeAsync([superuser, regularUser], cancellationToken);

        var users = new UserBuilder().Build(3);
        await context.AddRangeAsync(users, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    private User CreateSuperuser()
    {
        var superuser = new User
        {
            Id = Guid.Parse("019981b1-4595-7347-a777-8ba34a4c8952"),
            Username = "Bear",
            Email = "bear@bear.com",
            HashedPassword = passwordHasher.Hash("password"),
            UserPermissions = [],
        };

        foreach (var permission in Enum.GetValues(typeof(Permission)).Cast<Permission>())
        {
            superuser.UserPermissions.Add(new() { Permission = permission });
        }

        return superuser;
    }

    private User CreateRegularUser()
    {
        var regularUser = new User
        {
            Username = "AverageBear",
            Email = "bear@regular.com",
            HashedPassword = passwordHasher.Hash("password"),
            UserPermissions =
            [
                new()
                {
                    Permission = Permission.ViewListings,
                },
                new()
                {
                    Permission = Permission.ViewModels,
                },
                new()
                {
                    Permission = Permission.ViewCars,
                },
                new()
                {
                    Permission = Permission.CreateListings,
                }
            ],
        };

        return regularUser;
    }
}