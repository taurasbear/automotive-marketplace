using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
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

        var user = new User
        {
            Username = "Bear",
            Email = "bear@bear.com",
            HashedPassword = passwordHasher.Hash("password"),
        };
        await context.AddAsync(user, cancellationToken);

        var users = new UserBuilder().Build(3);
        await context.AddRangeAsync(users, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}