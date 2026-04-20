using AutoMapper;
using Automotive.Marketplace.Application.Features.MakeFeatures.UpdateMake;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.MakeHandlerTests;

public class UpdateMakeCommandHandlerTests(
    DatabaseFixture<UpdateMakeCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<UpdateMakeCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<UpdateMakeCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private UpdateMakeCommandHandler CreateHandler(IServiceScope scope)
    {
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository>();
        return new UpdateMakeCommandHandler(mapper, repository);
    }

    [Fact]
    public async Task Handle_ExistingMake_ShouldUpdateName()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var handler = CreateHandler(scope);

        var make = new MakeBuilder().With(m => m.Name, "OldName").Build();
        await context.AddAsync(make);
        await context.SaveChangesAsync();

        await handler.Handle(new UpdateMakeCommand { Id = make.Id, Name = "NewName" }, CancellationToken.None);

        var updated = await context.Set<Make>().AsNoTracking().FirstAsync();
        updated.Name.Should().Be("NewName");
    }
}
