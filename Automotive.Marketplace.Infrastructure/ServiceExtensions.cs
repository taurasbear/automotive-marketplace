namespace Automotive.Marketplace.Infrastructure;

using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Infrastructure.Data.DbContext;
using Automotive.Marketplace.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceExtensions
{
    public static void ConfigureInfrastructure(this IServiceCollection services, string? connectionString)
    {
        services.AddDbContext<AutomotiveContext>(opt => opt
            .UseLazyLoadingProxies()
            .UseNpgsql(connectionString));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
    }
}
