using Automotive.Marketplace.Application.Features.UserPreferencesFeatures.UpsertUserPreferences;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.UserPreferencesHandlerTests;

public class UpsertUserPreferencesCommandHandlerTests(
    DatabaseFixture<UpsertUserPreferencesCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<UpsertUserPreferencesCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<UpsertUserPreferencesCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private UpsertUserPreferencesCommandHandler CreateHandler(IServiceScope scope) =>
        new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_NoExistingPreferences_CreatesNewRow()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var user = new UserBuilder().Build();
        await context.AddAsync(user);
        await context.SaveChangesAsync();

        await handler.Handle(new UpsertUserPreferencesCommand
        {
            UserId = user.Id,
            ValueWeight = 0.40,
            EfficiencyWeight = 0.30,
            ReliabilityWeight = 0.20,
            MileageWeight = 0.10,
            AutoGenerateAiSummary = true,
        }, CancellationToken.None);

        var prefs = await context.UserPreferences.FirstOrDefaultAsync(p => p.UserId == user.Id);
        prefs.Should().NotBeNull();
        prefs!.ValueWeight.Should().Be(0.40);
        prefs.AutoGenerateAiSummary.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ExistingPreferences_UpdatesRow()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var user = new UserBuilder().Build();
        await context.AddAsync(user);
        var existing = new UserPreferences
        {
            Id = Guid.NewGuid(), UserId = user.Id,
            ValueWeight = 0.30, EfficiencyWeight = 0.25,
            ReliabilityWeight = 0.25, MileageWeight = 0.20,
        };
        await context.AddAsync(existing);
        await context.SaveChangesAsync();

        await handler.Handle(new UpsertUserPreferencesCommand
        {
            UserId = user.Id,
            ValueWeight = 0.50,
            EfficiencyWeight = 0.20,
            ReliabilityWeight = 0.20,
            MileageWeight = 0.10,
            AutoGenerateAiSummary = false,
        }, CancellationToken.None);

        context.ChangeTracker.Clear();
        var updated = await context.UserPreferences.FirstAsync(p => p.UserId == user.Id);
        updated.ValueWeight.Should().Be(0.50);
        updated.MileageWeight.Should().Be(0.10);
    }

    [Fact]
    public async Task Handle_InvalidWeightSum_ThrowsArgumentException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var user = new UserBuilder().Build();
        await context.AddAsync(user);
        await context.SaveChangesAsync();

        var act = async () => await handler.Handle(new UpsertUserPreferencesCommand
        {
            UserId = user.Id,
            ValueWeight = 0.50,
            EfficiencyWeight = 0.50,
            ReliabilityWeight = 0.50,
            MileageWeight = 0.50,
        }, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }
}
