using Testcontainers.PostgreSql;

namespace Automotive.Marketplace.Tests.Infrastructure;

public static class DatabaseContainer
{
    private static readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
            .WithDatabase("automotive_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();

    private static bool _isStarted = false;

    public static async Task<string> GetConnectionStringAsync()
    {
        if (!_isStarted)
        {
            await _container.StartAsync();
            _isStarted = true;
        }

        var connectionString = _container.GetConnectionString();
        return $"{connectionString};Include Error Detail=true";
    }

    public static async Task DisposeAsync()
    {
        if (_isStarted)
        {
            await _container.DisposeAsync();
        }
    }
}