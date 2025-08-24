using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Interfaces.Services;

public interface ITokenService
{
    public string GenerateAccessToken(User account);

    public string GenerateRefreshToken();

    public RefreshToken GenerateRefreshTokenEntity(User account);

    public DateTime GetRefreshTokenExpiryData();
}
