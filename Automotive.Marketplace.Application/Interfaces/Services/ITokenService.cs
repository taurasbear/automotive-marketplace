using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Interfaces.Services;

public interface ITokenService
{
    public string GenerateAccessToken(User user);

    public string GenerateRefreshToken();

    public RefreshToken GenerateRefreshTokenEntity(User user);

    public DateTime GetRefreshTokenExpiryData();
}
