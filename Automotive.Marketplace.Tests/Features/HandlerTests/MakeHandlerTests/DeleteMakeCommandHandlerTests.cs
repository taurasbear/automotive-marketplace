using Automotive.Marketplace.Application.Features.MakeFeatures.DeleteMake;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.MakeHandlerTests;

public class DeleteMakeCommandHandlerTests(
    DatabaseFixture<DeleteMakeCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<DeleteMakeCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<DeleteMakeCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private DeleteMakeCommandHandler CreateHandler(IServiceScope scope)
    {
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new DeleteMakeCommandHandler(repository);
    }

    [Fact]
    public async Task Handle_ExistingMake_ShouldRemoveMake()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        var make = new MakeBuilder().Build();
        await context.AddAsync(make);
        await context.SaveChangesAsync();

        await handler.Handle(new DeleteMakeCommand { Id = make.Id }, CancellationToken.None);

        var makes = await context.Set<Make>().ToListAsync();
        makes.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NonExistentId_ShouldThrowException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var act = async () => await handler.Handle(new DeleteMakeCommand { Id = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }
}
