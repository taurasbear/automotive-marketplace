using Amazon.S3;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Infrastructure.Data;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Data.Seeders;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Automotive.Marketplace.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Infrastructure;

public static class ServiceExtensions
{
    public static void ConfigureInfrastructure(this IServiceCollection services, IConfiguration configuration, string? connectionString)
    {
        services.AddDbContext<AutomotiveContext>(opt => opt
            .UseLazyLoadingProxies()
            .UseNpgsql(connectionString));
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IRepository, Repository>();
        services.AddScoped<IImageStorageService, S3ImageStorageService>();

        services.AddScoped<IDevelopmentSeeder, UserSeeder>();
        services.AddScoped<IDevelopmentSeeder, MakeSeeder>();
        services.AddScoped<IDevelopmentSeeder, ModelSeeder>();
        services.AddScoped<IDevelopmentSeeder, CarSeeder>();
        services.AddScoped<IDevelopmentSeeder, ListingSeeder>();

        var minioServerURL = configuration["MinIO:ServerURL"];
        var accessKey = configuration["MinIO:AccessKey"];
        var secretKey = configuration["MinIO:SecretKey"];

        services.AddSingleton<IAmazonS3>(opt =>
        {
            var config = new AmazonS3Config
            {
                ServiceURL = minioServerURL,
                ForcePathStyle = true,
                UseHttp = true,
            };

            return new AmazonS3Client(accessKey, secretKey, config);
        });
    }
}
