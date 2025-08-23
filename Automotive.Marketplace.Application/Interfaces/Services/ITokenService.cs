using System.Security.Claims;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Interfaces.Services;

public interface ITokenService
{
    public string GenerateAccessToken(Account account);

    public string GenerateRefreshToken();

    public RefreshToken GenerateRefreshTokenEntity(Account account);

    public DateTime GetRefreshTokenExpiryData();
}
