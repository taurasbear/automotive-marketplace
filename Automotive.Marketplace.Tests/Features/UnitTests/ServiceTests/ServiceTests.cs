using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Constants;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Automotive.Marketplace.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Xunit;

namespace Automotive.Marketplace.Tests.Features.UnitTests.ServiceTests;

public class ServiceTests
{
    #region PasswordHasher Tests

    [Fact]
    public void Hash_ReturnsNonNullHash()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        var password = "testPassword123!";

        // Act
        var hash = passwordHasher.Hash(password);

        // Assert
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        var password = "testPassword123!";
        var hash = passwordHasher.Hash(password);

        // Act
        var result = passwordHasher.Verify(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        var password = "testPassword123!";
        var wrongPassword = "wrongPassword456!";
        var hash = passwordHasher.Hash(password);

        // Act
        var result = passwordHasher.Verify(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region TokenService Tests

    [Fact]
    public void GenerateAccessToken_ReturnsValidJwt()
    {
        // Arrange
        var tokenService = new TokenService(CreateTestConfiguration());
        var user = CreateTestUser();

        // Act
        var token = tokenService.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        jwtToken.Should().NotBeNull();
        jwtToken.Claims.Should().NotBeEmpty();
    }

    [Fact]
    public void GenerateAccessToken_ContainsUserClaims()
    {
        // Arrange
        var tokenService = new TokenService(CreateTestConfiguration());
        var user = CreateTestUser();

        // Act
        var token = tokenService.GenerateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        
        var emailClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email);
        var idClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
        
        emailClaim.Should().NotBeNull();
        emailClaim!.Value.Should().Be(user.Email);
        
        idClaim.Should().NotBeNull();
        idClaim!.Value.Should().Be(user.Id.ToString());
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsBase64String()
    {
        // Arrange
        var tokenService = new TokenService(CreateTestConfiguration());

        // Act
        var token = tokenService.GenerateRefreshToken();

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        // Verify it's a valid base64 string
        var action = () => Convert.FromBase64String(token);
        action.Should().NotThrow();
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsDifferentTokensEachCall()
    {
        // Arrange
        var tokenService = new TokenService(CreateTestConfiguration());

        // Act
        var token1 = tokenService.GenerateRefreshToken();
        var token2 = tokenService.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GenerateRefreshTokenEntity_ReturnsCorrectEntity()
    {
        // Arrange
        var tokenService = new TokenService(CreateTestConfiguration());
        var user = CreateTestUser();
        var beforeCreation = DateTime.UtcNow;

        // Act
        var refreshTokenEntity = tokenService.GenerateRefreshTokenEntity(user);
        var afterCreation = DateTime.UtcNow;

        // Assert
        refreshTokenEntity.Should().NotBeNull();
        refreshTokenEntity.Token.Should().NotBeNullOrEmpty();
        refreshTokenEntity.IsRevoked.Should().BeFalse();
        refreshTokenEntity.IsUsed.Should().BeFalse();
        refreshTokenEntity.User.Should().Be(user);
        
        // Verify expiry date is in the future (within ~7 days)
        refreshTokenEntity.ExpiryDate.Should().BeAfter(afterCreation);
        refreshTokenEntity.ExpiryDate.Should().BeBefore(afterCreation.AddDays(7).AddMinutes(1));
    }

    #endregion

    #region Helper Methods

    private static IConfiguration CreateTestConfiguration()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:Key", "ThisIsATestKeyThatIsLongEnoughForHmacSha256!!" },
            { "Jwt:Issuer", "TestIssuer" },
            { "Jwt:Audience", "TestAudience" },
            { "Jwt:AccessTokenExpirationMinutes", "15" },
            { "Jwt:RefreshTokenExpirationDays", "7" },
        };
        return new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings).Build();
    }

    private static User CreateTestUser()
    {
        return new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            Username = "testuser",
            HashedPassword = "hash",
            UserPermissions = new List<UserPermission>
            {
                new() { Permission = Permission.ViewListings }
            }
        };
    }

    #endregion
}
