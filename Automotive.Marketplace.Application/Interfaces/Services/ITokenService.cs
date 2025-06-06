namespace Automotive.Marketplace.Application.Interfaces.Services;

using Automotive.Marketplace.Domain.Entities;

public interface ITokenService
{
    public string GenerateAccessToken(Account account);

    public string GenerateRefreshToken();

    public DateTime GetRefreshTokenExpiryData();
}
