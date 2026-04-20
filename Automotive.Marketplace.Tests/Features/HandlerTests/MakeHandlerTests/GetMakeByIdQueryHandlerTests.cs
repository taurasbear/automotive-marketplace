using AutoMapper;
using Automotive.Marketplace.Application.Features.MakeFeatures.GetMakeById;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.MakeHandlerTests;

public class GetMakeByIdQueryHandlerTests(
    DatabaseFixture<GetMakeByIdQueryHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<GetMakeByIdQueryHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<GetMakeByIdQueryHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private GetMakeByIdQueryHandler CreateHandler(IServiceScope scope)
    {
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new GetMakeByIdQueryHandler(mapper, repository);
    }

    [Fact]
    public async Task Handle_ExistingMake_ShouldReturnCorrectMake()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        var make = new MakeBuilder().Build();
        await context.AddAsync(make);
        await context.SaveChangesAsync();

        var result = await handler.Handle(new GetMakeByIdQuery { Id = make.Id }, CancellationToken.None);

        result.Id.Should().Be(make.Id);
        result.Name.Should().Be(make.Name);
    }

    [Fact]
    public async Task Handle_NonExistentId_ShouldThrowException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var act = async () => await handler.Handle(new GetMakeByIdQuery { Id = Guid.NewGuid() }, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }
}
