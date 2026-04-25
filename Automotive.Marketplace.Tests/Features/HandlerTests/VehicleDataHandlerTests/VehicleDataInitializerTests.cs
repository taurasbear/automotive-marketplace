using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Sync;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.VehicleDataHandlerTests;

public class VehicleDataInitializerTests(
    DatabaseFixture<VehicleDataInitializerTests> fixture)
    : IClassFixture<DatabaseFixture<VehicleDataInitializerTests>>, IAsyncLifetime
{
    private readonly IVehicleDataApiClient _apiClient = Substitute.For<IVehicleDataApiClient>();

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await fixture.ResetDatabaseAsync();

    private VehicleDataInitializer CreateInitializer(AutomotiveContext context)
        => new(context, _apiClient, NullLogger<VehicleDataInitializer>.Instance);

    [Fact]
    public async Task RunAsync_EmptyTable_ShouldFetchAndInsertMakesAndModels()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var makeDto = new VpicMakeDto(474, "HONDA");
        var modelDto = new VpicModelDto(1861, "Accord", 474);
        _apiClient.FetchCarMakesAsync(Arg.Any<CancellationToken>()).Returns([makeDto]);
        _apiClient.FetchModelsForMakeAsync(474, Arg.Any<CancellationToken>()).Returns([modelDto]);

        await CreateInitializer(context).RunAsync();

        var makes = context.Set<Make>().ToList();
        var models = context.Set<Model>().ToList();

        makes.Should().HaveCount(1);
        makes[0].VpicId.Should().Be(474);
        makes[0].VpicName.Should().Be("HONDA");
        makes[0].Name.Should().Be("Honda");
        makes[0].SyncedAt.Should().NotBeNull();
        makes[0].SyncedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        models.Should().HaveCount(1);
        models[0].VpicId.Should().Be(1861);
        models[0].VpicName.Should().Be("Accord");
        models[0].Name.Should().Be("Accord");
        models[0].MakeId.Should().Be(makes[0].Id);
        models[0].SyncedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RunAsync_FreshData_ShouldSkipApiCall()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        await context.Set<Make>().AddAsync(new Make
        {
            Id = Guid.NewGuid(),
            Name = "Honda",
            VpicId = 474,
            VpicName = "HONDA",
            SyncedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        });
        await context.SaveChangesAsync();

        await CreateInitializer(context).RunAsync();

        await _apiClient.DidNotReceive().FetchCarMakesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_StaleData_ShouldUpdateExistingMake()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var existingId = Guid.NewGuid();
        await context.Set<Make>().AddAsync(new Make
        {
            Id = existingId,
            Name = "Old Name",
            VpicId = 474,
            VpicName = "OLD",
            SyncedAt = DateTime.UtcNow.AddDays(-31),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system"
        });
        await context.SaveChangesAsync();

        _apiClient.FetchCarMakesAsync(Arg.Any<CancellationToken>())
            .Returns([new VpicMakeDto(474, "HONDA")]);
        _apiClient.FetchModelsForMakeAsync(474, Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<VpicModelDto>());

        await CreateInitializer(context).RunAsync();

        var updated = context.Set<Make>().Single(m => m.Id == existingId);
        updated.Name.Should().Be("Honda");
        updated.VpicName.Should().Be("HONDA");
        updated.SyncedAt!.Value.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        updated.Id.Should().Be(existingId);
    }

    [Fact]
    public async Task RunAsync_ExcludedMake_ShouldNotBeUpserted()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        await context.Set<MakeExclusion>().AddAsync(new MakeExclusion
        {
            VpicId = 465,
            VpicName = "MERCURY"
        });
        await context.SaveChangesAsync();

        _apiClient.FetchCarMakesAsync(Arg.Any<CancellationToken>())
            .Returns([new VpicMakeDto(465, "MERCURY"), new VpicMakeDto(474, "HONDA")]);
        _apiClient.FetchModelsForMakeAsync(474, Arg.Any<CancellationToken>())
            .Returns(Enumerable.Empty<VpicModelDto>());

        await CreateInitializer(context).RunAsync();

        var makes = context.Set<Make>().ToList();
        makes.Should().HaveCount(1);
        makes[0].VpicId.Should().Be(474);
        await _apiClient.DidNotReceive().FetchModelsForMakeAsync(465, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_ApiFailure_ShouldLogAndNotThrow()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        _apiClient.FetchCarMakesAsync(Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IEnumerable<VpicMakeDto>>(new HttpRequestException("Network error")));

        var act = async () => await CreateInitializer(context).RunAsync();

        await act.Should().NotThrowAsync();
        context.Set<Make>().Should().BeEmpty();
    }
}
