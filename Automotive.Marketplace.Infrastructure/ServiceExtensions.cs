using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Infrastructure.Data;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Data.Seeders;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Automotive.Marketplace.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModelEntityBuilder = Automotive.Marketplace.Infrastructure.Data.Builders.ModelBuilder;

namespace Automotive.Marketplace.Infrastructure;

public static class ServiceExtensions
{
    public static void ConfigureInfrastructure(this IServiceCollection services, string? connectionString)
    {
        services.AddDbContext<AutomotiveContext>(opt => opt
            .UseLazyLoadingProxies()
            .UseNpgsql(connectionString));
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IRepository, Repository>();

        services.AddScoped<IDevelopmentSeeder, UserSeeder>();
        services.AddScoped<IDevelopmentSeeder, MakeSeeder>();
        services.AddScoped<IDevelopmentSeeder, ModelSeeder>();
        services.AddScoped<IDevelopmentSeeder, CarSeeder>();
        services.AddScoped<IDevelopmentSeeder, CarDetailsSeeder>();
        services.AddScoped<IDevelopmentSeeder, ListingSeeder>();
    }
}
