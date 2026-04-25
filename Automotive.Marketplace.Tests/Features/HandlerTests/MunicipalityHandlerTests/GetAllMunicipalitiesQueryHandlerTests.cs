using Automotive.Marketplace.Application.Features.MunicipalityFeatures.GetAllMunicipalities;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.MunicipalityHandlerTests;

public class GetAllMunicipalitiesQueryHandlerTests(
    DatabaseFixture<GetAllMunicipalitiesQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetAllMunicipalitiesQueryHandlerTests>>, IAsyncLifetime
{
    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await fixture.ResetDatabaseAsync();

    private GetAllMunicipalitiesQueryHandler CreateHandler(IServiceScope scope)
    {
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new GetAllMunicipalitiesQueryHandler(repository);
    }

    [Fact]
    public async Task Handle_NoMunicipalities_ShouldReturnEmpty()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var result = await handler.Handle(new GetAllMunicipalitiesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WithMunicipalities_ShouldReturnAllSortedAlphabetically()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        await context.AddRangeAsync(
            new Municipality { Id = Guid.NewGuid(), Name = "Vilniaus m.", SyncedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, CreatedBy = "test" },
            new Municipality { Id = Guid.NewGuid(), Name = "Alytaus m.", SyncedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, CreatedBy = "test" },
            new Municipality { Id = Guid.NewGuid(), Name = "Klaipėdos m.", SyncedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, CreatedBy = "test" });
        await context.SaveChangesAsync();

        var result = (await handler.Handle(new GetAllMunicipalitiesQuery(), CancellationToken.None)).ToList();

        result.Should().HaveCount(3);
        result[0].Name.Should().Be("Alytaus m.");
        result[1].Name.Should().Be("Klaipėdos m.");
        result[2].Name.Should().Be("Vilniaus m.");
    }

    [Fact]
    public async Task Handle_WithMunicipalities_ShouldReturnCorrectIdAndName()
    {
        await using var scope = fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var id = Guid.NewGuid();
        await context.AddAsync(new Municipality { Id = id, Name = "Kauno m.", SyncedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, CreatedBy = "test" });
        await context.SaveChangesAsync();

        var result = (await handler.Handle(new GetAllMunicipalitiesQuery(), CancellationToken.None)).Single();

        result.Id.Should().Be(id);
        result.Name.Should().Be("Kauno m.");
    }
}
