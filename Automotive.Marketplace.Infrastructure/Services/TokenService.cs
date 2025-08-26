using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Constants;
using Automotive.Marketplace.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Automotive.Marketplace.Infrastructure.Services;

public class TokenService(IConfiguration configuration) : ITokenService
{
    private readonly IConfiguration configuration = configuration;

    public string GenerateAccessToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this.configuration["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        };

        var userPermissions = user.UserPermissions
            .Select(userPermission => userPermission.Permission)
            .ToList();

        foreach (var permission in userPermissions)
        {
            claims.Add(new Claim(CustomClaimType.Permissions, permission.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: this.configuration["Jwt:Issuer"],
            audience: this.configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                Convert.ToDouble(this.configuration["Jwt:AccessTokenExpirationMinutes"])
                ),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new Byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }

    public RefreshToken GenerateRefreshTokenEntity(User user)
    {
        var freshRefreshToken = this.GenerateRefreshToken();
        var freshExpiryDate = this.GetRefreshTokenExpiryData();

        return new RefreshToken
        {
            Token = freshRefreshToken,
            ExpiryDate = freshExpiryDate,
            IsRevoked = false,
            IsUsed = false,
            User = user
        };
    }

    public DateTime GetRefreshTokenExpiryData()
    {
        return DateTime.UtcNow
            .AddDays(Convert.ToDouble(this.configuration["Jwt:RefreshTokenExpirationDays"]));
    }
}
