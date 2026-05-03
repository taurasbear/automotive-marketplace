using AutoMapper;
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.AuthFeatures.RefreshToken;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using RefreshTokenEntity = Automotive.Marketplace.Domain.Entities.RefreshToken;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.AuthHandlerTests;

public class RefreshTokenCommandHandlerTests(
    DatabaseFixture<RefreshTokenCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<RefreshTokenCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<RefreshTokenCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private static (RefreshTokenCommandHandler handler, ITokenService tokenService) CreateHandler(
        IServiceScope scope)
    {
        var tokenService = Substitute.For<ITokenService>();
        var handler = new RefreshTokenCommandHandler(
            scope.ServiceProvider.GetRequiredService<IMapper>(),
            tokenService,
            scope.ServiceProvider.GetRequiredService<IRepository>());
        return (handler, tokenService);
    }

    [Fact]
    public async Task Handle_ValidToken_ReturnsNewTokensAndRevokesOld()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var user = new UserBuilder().Build();
        await context.AddAsync(user);

        var refreshToken = new RefreshTokenEntity
        {
            Token = "valid_refresh",
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            IsUsed = false,
            User = user,
        };
        await context.AddAsync(refreshToken);
        await context.SaveChangesAsync();

        var (handler, tokenService) = CreateHandler(scope);
        tokenService.GenerateAccessToken(Arg.Any<User>()).Returns("new_access");
        tokenService.GenerateRefreshTokenEntity(Arg.Any<User>()).Returns(callInfo =>
        {
            var u = callInfo.Arg<User>();
            return new RefreshTokenEntity
            {
                Token = "new_refresh",
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                IsUsed = false,
                User = u,
            };
        });

        var result = await handler.Handle(
            new RefreshTokenCommand { RefreshToken = "valid_refresh" },
            CancellationToken.None);

        result.FreshAccessToken.Should().Be("new_access");
        result.FreshRefreshToken.Should().Be("new_refresh");

        var old = await context.Set<RefreshTokenEntity>().AsNoTracking()
            .FirstAsync(rt => rt.Token == "valid_refresh");
        old.IsRevoked.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_InvalidToken_ThrowsInvalidRefreshTokenException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var (handler, _) = CreateHandler(scope);

        var act = () => handler.Handle(
            new RefreshTokenCommand { RefreshToken = "nonexistent" },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }

    [Fact]
    public async Task Handle_ExpiredToken_ThrowsInvalidRefreshTokenException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var user = new UserBuilder().Build();
        await context.AddAsync(user);

        var refreshToken = new RefreshTokenEntity
        {
            Token = "expired_refresh",
            ExpiryDate = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false,
            IsUsed = false,
            User = user,
        };
        await context.AddAsync(refreshToken);
        await context.SaveChangesAsync();

        var (handler, _) = CreateHandler(scope);

        var act = () => handler.Handle(
            new RefreshTokenCommand { RefreshToken = "expired_refresh" },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }

    [Fact]
    public async Task Handle_RevokedToken_ThrowsInvalidRefreshTokenException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var user = new UserBuilder().Build();
        await context.AddAsync(user);

        var refreshToken = new RefreshTokenEntity
        {
            Token = "revoked_refresh",
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = true,
            IsUsed = false,
            User = user,
        };
        await context.AddAsync(refreshToken);
        await context.SaveChangesAsync();

        var (handler, _) = CreateHandler(scope);

        var act = () => handler.Handle(
            new RefreshTokenCommand { RefreshToken = "revoked_refresh" },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidRefreshTokenException>();
    }
}
