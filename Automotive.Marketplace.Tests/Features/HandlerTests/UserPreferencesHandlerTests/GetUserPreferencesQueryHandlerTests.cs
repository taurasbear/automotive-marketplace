using Automotive.Marketplace.Application.Features.UserPreferencesFeatures.GetUserPreferences;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.UserPreferencesHandlerTests;

public class GetUserPreferencesQueryHandlerTests(
    DatabaseFixture<GetUserPreferencesQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetUserPreferencesQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetUserPreferencesQueryHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetUserPreferencesQueryHandler CreateHandler(IServiceScope scope) =>
        new(scope.ServiceProvider.GetRequiredService<IRepository>());

    [Fact]
    public async Task Handle_NoPreferencesExist_ReturnsDefaultWeights()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var result = await handler.Handle(new GetUserPreferencesQuery { UserId = Guid.NewGuid() }, CancellationToken.None);

        result.HasPreferences.Should().BeFalse();
        result.ValueWeight.Should().Be(0.30);
        result.EfficiencyWeight.Should().Be(0.25);
        result.ReliabilityWeight.Should().Be(0.25);
        result.MileageWeight.Should().Be(0.20);
        result.AutoGenerateAiSummary.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_PreferencesExist_ReturnsStoredWeights()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var user = new UserBuilder().Build();
        await context.AddAsync(user);
        var prefs = new UserPreferences
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            ValueWeight = 0.40,
            EfficiencyWeight = 0.30,
            ReliabilityWeight = 0.20,
            MileageWeight = 0.10,
            AutoGenerateAiSummary = true,
        };
        await context.AddAsync(prefs);
        await context.SaveChangesAsync();

        var result = await handler.Handle(new GetUserPreferencesQuery { UserId = user.Id }, CancellationToken.None);

        result.HasPreferences.Should().BeTrue();
        result.ValueWeight.Should().Be(0.40);
        result.EfficiencyWeight.Should().Be(0.30);
        result.ReliabilityWeight.Should().Be(0.20);
        result.MileageWeight.Should().Be(0.10);
        result.AutoGenerateAiSummary.Should().BeTrue();
    }
}
