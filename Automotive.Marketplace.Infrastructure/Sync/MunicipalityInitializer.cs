using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Automotive.Marketplace.Infrastructure.Sync;

public class MunicipalityInitializer(
    AutomotiveContext context,
    IMunicipalityApiClient apiClient,
    ILogger<MunicipalityInitializer> logger) : IMunicipalityInitializer
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromDays(30);

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var hasAny = await context.Set<Municipality>().AnyAsync(cancellationToken);
            var isStale = !hasAny
                || await context.Set<Municipality>().MinAsync(m => m.SyncedAt, cancellationToken)
                   < DateTime.UtcNow - SyncInterval;

            if (!isStale)
            {
                logger.LogInformation("Municipality data is fresh, skipping sync.");
                return;
            }

            logger.LogInformation("Syncing municipality data from government API...");
            var municipalities = (await apiClient.FetchMunicipalitiesAsync(cancellationToken)).ToList();
            var syncedAt = DateTime.UtcNow;

            foreach (var dto in municipalities)
            {
                var existing = await context.Set<Municipality>()
                    .FindAsync([dto.Id], cancellationToken);

                if (existing is null)
                    await context.Set<Municipality>().AddAsync(
                        new Municipality { Id = dto.Id, Name = dto.Name, SyncedAt = syncedAt, CreatedAt = syncedAt, CreatedBy = "system" },
                        cancellationToken);
                else
                {
                    existing.Name = dto.Name;
                    existing.SyncedAt = syncedAt;
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Municipality sync complete. {Count} records upserted.", municipalities.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Municipality sync failed. App will start with existing data.");
        }
    }
}
