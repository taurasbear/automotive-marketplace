using AutoMapper;
using Automotive.Marketplace.Application.Features.MakeFeatures.CreateMake;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.MakeHandlerTests;

public class CreateMakeCommandHandlerTests(
    DatabaseFixture<CreateMakeCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<CreateMakeCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<CreateMakeCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private CreateMakeCommandHandler CreateHandler(IServiceScope scope)
    {
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new CreateMakeCommandHandler(mapper, repository);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldPersistMake()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        await handler.Handle(new CreateMakeCommand { Name = "Toyota" }, CancellationToken.None);

        var makes = await context.Set<Make>().ToListAsync();
        makes.Should().HaveCount(1);
        makes.First().Name.Should().Be("Toyota");
    }
}
