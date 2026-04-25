using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Automotive.Marketplace.Infrastructure.Sync;

public class VehicleDataInitializer(
    AutomotiveContext context,
    IVehicleDataApiClient apiClient,
    ILogger<VehicleDataInitializer> logger) : IVehicleDataInitializer
{
    private static readonly TimeSpan SyncInterval = TimeSpan.FromDays(30);

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var hasSynced = await context.Set<Make>()
                .AnyAsync(m => m.SyncedAt.HasValue, cancellationToken);

            var isStale = !hasSynced
                || await context.Set<Make>()
                    .Where(m => m.SyncedAt.HasValue)
                    .MinAsync(m => m.SyncedAt!.Value, cancellationToken)
                   < DateTime.UtcNow - SyncInterval;

            if (!isStale)
            {
                logger.LogInformation("Vehicle data is fresh, skipping sync.");
                return;
            }

            logger.LogInformation("Syncing vehicle data from vPIC API...");
            var syncedAt = DateTime.UtcNow;

            var allMakes = (await apiClient.FetchCarMakesAsync(cancellationToken)).ToList();
            logger.LogInformation("Fetched {Count} makes from vPIC.", allMakes.Count);

            var excludedVpicIds = await context.Set<MakeExclusion>()
                .Select(e => e.VpicId)
                .ToHashSetAsync(cancellationToken);

            var allowedMakes = allMakes.Where(m => !excludedVpicIds.Contains(m.VpicId)).ToList();
            logger.LogInformation("{Allowed} makes allowed after exclusion filter.", allowedMakes.Count);

            var existingMakesByVpicId = await context.Set<Make>()
                .Where(m => m.VpicId.HasValue)
                .ToDictionaryAsync(m => m.VpicId!.Value, cancellationToken);

            var upsertedMakes = new Dictionary<int, Guid>();

            foreach (var dto in allowedMakes)
            {
                if (existingMakesByVpicId.TryGetValue(dto.VpicId, out var existing))
                {
                    existing.Name = ToTitleCase(dto.VpicName);
                    existing.VpicName = dto.VpicName;
                    existing.SyncedAt = syncedAt;
                    upsertedMakes[dto.VpicId] = existing.Id;
                }
                else
                {
                    var newMake = new Make
                    {
                        Id = Guid.NewGuid(),
                        VpicId = dto.VpicId,
                        VpicName = dto.VpicName,
                        Name = ToTitleCase(dto.VpicName),
                        SyncedAt = syncedAt,
                        CreatedAt = syncedAt,
                        CreatedBy = "system"
                    };
                    await context.Set<Make>().AddAsync(newMake, cancellationToken);
                    upsertedMakes[dto.VpicId] = newMake.Id;
                }
            }

            await context.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Makes upserted: {Count}.", allowedMakes.Count);

            var semaphore = new SemaphoreSlim(10);
            var modelFetchTasks = allowedMakes.Select(async make =>
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var models = await apiClient.FetchModelsForMakeAsync(make.VpicId, cancellationToken);
                    return (make.VpicId, models.ToList());
                }
                finally
                {
                    semaphore.Release();
                }
            });

            var allMakeModels = await Task.WhenAll(modelFetchTasks);
            logger.LogInformation("Model data fetched for all makes.");

            var existingModelsByVpicId = await context.Set<Model>()
                .Where(m => m.VpicId.HasValue)
                .ToDictionaryAsync(m => m.VpicId!.Value, cancellationToken);

            foreach (var (makeVpicId, models) in allMakeModels)
            {
                if (!upsertedMakes.TryGetValue(makeVpicId, out var makeGuid))
                    continue;

                foreach (var dto in models)
                {
                    if (existingModelsByVpicId.TryGetValue(dto.VpicId, out var existingModel))
                    {
                        existingModel.Name = ToTitleCase(dto.VpicName);
                        existingModel.VpicName = dto.VpicName;
                        existingModel.SyncedAt = syncedAt;
                        existingModel.MakeId = makeGuid;
                    }
                    else
                    {
                        var newModel = new Model
                        {
                            Id = Guid.NewGuid(),
                            VpicId = dto.VpicId,
                            VpicName = dto.VpicName,
                            Name = ToTitleCase(dto.VpicName),
                            MakeId = makeGuid,
                            SyncedAt = syncedAt,
                            CreatedAt = syncedAt,
                            CreatedBy = "system"
                        };
                        await context.Set<Model>().AddAsync(newModel, cancellationToken);
                        existingModelsByVpicId[dto.VpicId] = newModel;
                    }
                }

                await context.SaveChangesAsync(cancellationToken);
            }

            logger.LogInformation("Vehicle data sync complete.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Vehicle data sync failed. App will start with existing data.");
        }
    }

    private static string ToTitleCase(string s) =>
        CultureInfo.InvariantCulture.TextInfo.ToTitleCase(s.ToLower());
}
