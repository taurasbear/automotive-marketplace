using Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Infrastructure.Data;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Respawn;

namespace Automotive.Marketplace.Tests.Infrastructure;

public class DatabaseFixture<T> : IAsyncLifetime where T : class
{
    private string _connectionString = null!;

    private string _databaseName = null!;

    private Respawner _respawner = null!;

    public ServiceProvider ServiceProvider { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var baseConnectionString = await DatabaseContainer.GetConnectionStringAsync();

        _databaseName = $"test_{typeof(T).Name.ToLower()}";
        _connectionString = baseConnectionString.Replace("automotive_test", _databaseName);

        var services = new ServiceCollection();
        services.AddDbContext<AutomotiveContext>(options =>
            options.UseNpgsql(_connectionString), ServiceLifetime.Transient);

        var handlerTypes = typeof(GetAllListingsQueryHandler).Assembly
            .GetTypes()
            .Where(t => t.Name.EndsWith("Handler") && !t.IsAbstract)
            .ToList();

        foreach (var handlerType in handlerTypes)
        {
            services.AddScoped(handlerType);
        }

        services.AddScoped<IRepository, Repository>();
        services.AddAutoMapper(typeof(GetAllListingsQueryHandler).Assembly);

        ServiceProvider = services.BuildServiceProvider();

        using var scope = ServiceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        await context.Database.EnsureCreatedAsync();

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            SchemasToInclude = ["public"],
            DbAdapter = DbAdapter.Postgres,
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        await _respawner.ResetAsync(connection);
    }

    public async Task DisposeAsync()
    {
        if (ServiceProvider != null)
        {
            await ServiceProvider.DisposeAsync();
        }
    }
}