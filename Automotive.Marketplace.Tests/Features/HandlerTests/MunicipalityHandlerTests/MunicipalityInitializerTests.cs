using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Infrastructure.Sync;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.MunicipalityHandlerTests;

public class MunicipalityInitializerTests(
    DatabaseFixture<MunicipalityInitializerTests> fixture)
    : IClassFixture<DatabaseFixture<MunicipalityInitializerTests>>, IAsyncLifetime
{
    private readonly IMunicipalityApiClient _apiClient = Substitute.For<IMunicipalityApiClient>();

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await fixture.ResetDatabaseAsync();

    private MunicipalityInitializer CreateInitializer(AutomotiveContext context)
        => new(context, _apiClient, NullLogger<MunicipalityInitializer>.Instance);

    [Fact]
    public async Task RunAsync_EmptyTable_ShouldFetchAndInsertAllRecords()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var dto1 = new MunicipalityDto(Guid.NewGuid(), "Vilniaus m.");
        var dto2 = new MunicipalityDto(Guid.NewGuid(), "Kauno m.");
        _apiClient.FetchMunicipalitiesAsync(Arg.Any<CancellationToken>())
            .Returns([dto1, dto2]);

        await CreateInitializer(context).RunAsync();

        var saved = context.Set<Municipality>().ToList();
        saved.Should().HaveCount(2);
        saved.Should().ContainSingle(m => m.Id == dto1.Id && m.Name == dto1.Name);
        saved.Should().ContainSingle(m => m.Id == dto2.Id && m.Name == dto2.Name);
    }

    [Fact]
    public async Task RunAsync_FreshData_ShouldSkipApiCall()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        await context.AddAsync(new Municipality
        {
            Id = Guid.NewGuid(),
            Name = "Vilniaus m.",
            SyncedAt = DateTime.UtcNow, // fresh
            CreatedAt = DateTime.UtcNow, // REQUIRED: BaseEntity non-nullable
            CreatedBy = "test"           // REQUIRED: BaseEntity non-nullable
        });
        await context.SaveChangesAsync();

        await CreateInitializer(context).RunAsync();

        await _apiClient.DidNotReceive().FetchMunicipalitiesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunAsync_StaleData_ShouldUpdateExistingRecord()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var existingId = Guid.NewGuid();
        await context.AddAsync(new Municipality
        {
            Id = existingId,
            Name = "Old Name",
            SyncedAt = DateTime.UtcNow.AddDays(-31), // stale
            CreatedAt = DateTime.UtcNow,  // REQUIRED: BaseEntity non-nullable
            CreatedBy = "test"            // REQUIRED: BaseEntity non-nullable
        });
        await context.SaveChangesAsync();

        _apiClient.FetchMunicipalitiesAsync(Arg.Any<CancellationToken>())
            .Returns([new MunicipalityDto(existingId, "Updated Name")]);

        await CreateInitializer(context).RunAsync();

        var updated = context.Set<Municipality>().Single(m => m.Id == existingId);
        updated.Name.Should().Be("Updated Name");
        updated.SyncedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
