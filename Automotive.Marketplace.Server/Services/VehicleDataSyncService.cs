using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Automotive.Marketplace.Server.Services;

public class VehicleDataSyncService(
    IServiceScopeFactory scopeFactory,
    ILogger<VehicleDataSyncService> logger) : BackgroundService
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
                var initializer = scope.ServiceProvider.GetRequiredService<IVehicleDataInitializer>();
                await initializer.RunAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Vehicle data sync check failed.");
            }
        }
    }
}
