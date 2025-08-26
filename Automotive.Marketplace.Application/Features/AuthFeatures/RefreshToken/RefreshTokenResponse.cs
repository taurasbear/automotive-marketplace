using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Features.AuthFeatures.RefreshToken;

public sealed record class RefreshTokenResponse
{
    public string FreshAccessToken { get; set; } = string.Empty;

    public string FreshRefreshToken { get; set; } = string.Empty;

    public DateTime FreshExpiryDate { get; set; }

    public Guid UserId { get; set; }

    public IEnumerable<Permission> Permissions { get; set; } = [];
}
