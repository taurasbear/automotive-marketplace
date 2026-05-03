using Automotive.Marketplace.Application.Features.AuthFeatures.LogoutUser;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.AuthHandlerTests;

public class LogoutUserCommandHandlerTests(
    DatabaseFixture<LogoutUserCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<LogoutUserCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<LogoutUserCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static LogoutUserCommandHandler CreateHandler(IServiceScope scope)
    {
        return new LogoutUserCommandHandler(
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    [Fact]
    public async Task Handle_ValidToken_RevokesRefreshToken()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var user = new UserBuilder().Build();
        await context.AddAsync(user);

        var refreshToken = new RefreshToken
        {
            Token = "valid_token",
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            IsUsed = false,
            User = user,
        };
        await context.AddAsync(refreshToken);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);

        await handler.Handle(new LogoutUserCommand { RefreshToken = "valid_token" }, CancellationToken.None);

        var updated = await context.Set<RefreshToken>().AsNoTracking()
            .FirstAsync(rt => rt.Token == "valid_token");
        updated.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ExpiredToken_DoesNotRevoke()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var user = new UserBuilder().Build();
        await context.AddAsync(user);

        var refreshToken = new RefreshToken
        {
            Token = "expired_token",
            ExpiryDate = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false,
            IsUsed = false,
            User = user,
        };
        await context.AddAsync(refreshToken);
        await context.SaveChangesAsync();

        var handler = CreateHandler(scope);

        await handler.Handle(new LogoutUserCommand { RefreshToken = "expired_token" }, CancellationToken.None);

        var notRevoked = await context.Set<RefreshToken>().AsNoTracking()
            .FirstAsync(rt => rt.Token == "expired_token");
        notRevoked.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_NonExistentToken_CompletesWithoutError()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var act = () => handler.Handle(new LogoutUserCommand { RefreshToken = "nonexistent" }, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
