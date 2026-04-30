using AutoMapper;
using Automotive.Marketplace.Application.Features.AuthFeatures.RegisterUser;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Infrastructure.Data.DatabaseContext;
using Automotive.Marketplace.Tests.Infrastructure;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Automotive.Marketplace.Tests.Features.HandlerTests.AuthHandlerTests;

public class RegisterUserCommandHandlerTests(
    DatabaseFixture<RegisterUserCommandHandlerTests> fixture)
    : IClassFixture<DatabaseFixture<RegisterUserCommandHandlerTests>>, IAsyncLifetime
{
    private readonly DatabaseFixture<RegisterUserCommandHandlerTests> _fixture = fixture;

    public Task InitializeAsync() => Task.CompletedTask;
    public async Task DisposeAsync() => await _fixture.ResetDatabaseAsync();

    private RegisterUserCommandHandler CreateHandler(IServiceScope scope)
    {
        var passwordHasher = Substitute.For<IPasswordHasher>();
        passwordHasher.Hash(Arg.Any<string>()).Returns("hashed_password");

        var tokenService = Substitute.For<ITokenService>();
        tokenService.GenerateAccessToken(Arg.Any<User>()).Returns("access_token");
        tokenService.GenerateRefreshTokenEntity(Arg.Any<User>()).Returns(callInfo =>
        {
            var user = callInfo.Arg<User>();
            return new RefreshToken
            {
                Token = "refresh_token",
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                IsUsed = false,
                User = user,
            };
        });

        return new RegisterUserCommandHandler(
            scope.ServiceProvider.GetRequiredService<IMapper>(),
            passwordHasher,
            tokenService,
            scope.ServiceProvider.GetRequiredService<IRepository>());
    }

    [Fact]
    public async Task Handle_NewUser_AssignsExactlyDefaultPermissions()
    {
        await using var scope = _fixture.ServiceProvider.CreateAsyncScope();
        var handler = CreateHandler(scope);
        var context = scope.ServiceProvider.GetRequiredService<AutomotiveContext>();

        var command = new RegisterUserCommand
        {
            Username = "testuser",
            Email = "testuser@example.com",
            Password = "Password123!",
        };

        var response = await handler.Handle(command, CancellationToken.None);

        response.AccessToken.Should().NotBeNullOrEmpty();
        response.RefreshToken.Should().NotBeNullOrEmpty();

        var user = await context.Set<User>()
            .Include(u => u.UserPermissions)
            .FirstOrDefaultAsync(u => u.Email == command.Email);

        user.Should().NotBeNull();
        user!.UserPermissions.Select(p => p.Permission)
            .Should().BeEquivalentTo(new[]
            {
                Permission.ViewListings,
                Permission.CreateListings,
                Permission.ManageListings,
                Permission.ViewModels,
                Permission.ViewVariants,
                Permission.ViewMakes,
            });
    }
}
