using AutoMapper;
using Automotive.Marketplace.Application.Features.MakeFeatures.GetAllMakes;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.MakeHandlerTests;

public class GetAllMakesQueryHandlerTests(
    DatabaseFixture<GetAllMakesQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetAllMakesQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetAllMakesQueryHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetAllMakesQueryHandler CreateHandler(IServiceScope scope)
    {
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new GetAllMakesQueryHandler(mapper, repository);
    }

    [Fact]
    public async Task Handle_WithMakes_ShouldReturnAllMakesOrderedByName()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        var makes = new MakeBuilder().Build(3);
        await context.AddRangeAsync(makes);
        await context.SaveChangesAsync();

        var result = await handler.Handle(new GetAllMakesQuery(), CancellationToken.None);
        var list = result.ToList();

        list.Should().HaveCount(3);
        list.Should().BeInAscendingOrder(m => m.Name);
    }

    [Fact]
    public async Task Handle_WithNoMakes_ShouldReturnEmptyCollection()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var result = await handler.Handle(new GetAllMakesQuery(), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldMapAuditFields()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        var make = new MakeBuilder()
            .With(m => m.CreatedBy, "admin")
            .With(m => m.CreatedAt, DateTime.UtcNow)
            .Build();
        await context.AddAsync(make);
        await context.SaveChangesAsync();

        var result = await handler.Handle(new GetAllMakesQuery(), CancellationToken.None);
        var returned = result.Single();

        returned.CreatedBy.Should().Be("admin");
        returned.CreatedAt.Should().NotBe(default);
    }
}
