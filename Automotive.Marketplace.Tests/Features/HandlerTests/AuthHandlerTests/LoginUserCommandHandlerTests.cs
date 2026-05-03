using AutoMapper;
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.AuthFeatures.LoginUser;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Data.Builders;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.AuthHandlerTests;

public class LoginUserCommandHandlerTests(
    DatabaseFixture<LoginUserCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<LoginUserCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<LoginUserCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private LoginUserCommandHandler CreateHandler(
        IServiceScope scope,
        IPasswordHasher? passwordHasher = null,
        ITokenService? tokenService = null)
    {
        var ph = passwordHasher ?? Substitute.For<IPasswordHasher>();
        var ts = tokenService ?? Substitute.For<ITokenService>();

        return new LoginUserCommandHandler(
            scope.ServiceProvider.GetRequiredService<IMapper>(),
            ph,
            ts,
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    private static async Task<User> SeedUserAsync(AutomotiveContext context, string email = "test@example.com")
    {
        var user = new UserBuilder()
            .With(u => u.Email, email)
            .With(u => u.HashedPassword, "hashed_pw")
            .Build();

        user.UserPermissions =
        [
            new UserPermission { Permission = Permission.ViewListings },
            new UserPermission { Permission = Permission.CreateListings },
        ];

        await context.AddAsync(user);
        await context.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task Handle_ValidCredentials_ReturnsTokensAndPermissions()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        var user = await SeedUserAsync(context);

        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(true);

        var tokenService = Substitute.For<ITokenService>();
        tokenService.GenerateAccessToken(Arg.Any<User>()).Returns("access_token");
        tokenService.GenerateRefreshTokenEntity(Arg.Any<User>()).Returns(callInfo =>
        {
            var u = callInfo.Arg<User>();
            return new RefreshToken
            {
                Token = "refresh_token",
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                IsUsed = false,
                User = u,
            };
        });

        var handler = CreateHandler(scope, passwordHasher, tokenService);

        var result = await handler.Handle(new LoginUserCommand
        {
            Email = user.Email,
            Password = "correct_password",
        }, CancellationToken.None);

        result.FreshAccessToken.Should().Be("access_token");
        result.FreshRefreshToken.Should().Be("refresh_token");
        result.UserId.Should().Be(user.Id);
        result.Permissions.Should().Contain(Permission.ViewListings);
    }

    [Fact]
    public async Task Handle_NonExistentEmail_ThrowsUserNotFoundException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);

        var act = () => handler.Handle(new LoginUserCommand
        {
            Email = "nobody@example.com",
            Password = "password",
        }, CancellationToken.None);

        await act.Should().ThrowAsync<UserNotFoundException>();
    }

    [Fact]
    public async Task Handle_WrongPassword_ThrowsInvalidCredentialsException()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();
        await SeedUserAsync(context);

        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Verify(Arg.Any<string>(), Arg.Any<string>()).Returns(false);

        var handler = CreateHandler(scope, passwordHasher);

        var act = () => handler.Handle(new LoginUserCommand
        {
            Email = "test@example.com",
            Password = "wrong_password",
        }, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }
}
