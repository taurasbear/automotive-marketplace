using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Automotive.Marketplace.Server.Services;

public class MunicipalitySyncService(
    IServiceScopeFactory scopeFactory,
    ILogger<MunicipalitySyncService> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(CheckInterval, stoppingToken);
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var initializer = scope.ServiceProvider.GetRequiredService<IMunicipalityInitializer>();
                await initializer.RunAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Municipality sync check failed.");
            }
        }
    }
}
