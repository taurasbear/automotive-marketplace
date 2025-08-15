namespace Automotive.Marketplace.Application.Features.AuthFeatures.RegisterAccount;

public sealed record RegisterAccountResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime RefreshTokenExpiryDate { get; set; }

    public Guid AccountId { get; set; }

    public string RoleName { get; set; } = string.Empty;
}
