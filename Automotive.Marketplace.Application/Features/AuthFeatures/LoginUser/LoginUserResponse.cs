namespace Automotive.Marketplace.Application.Features.AuthFeatures.AuthenticateAccount;

public sealed record LoginUserResponse
{
    public string FreshAccessToken { get; set; } = string.Empty;

    public string FreshRefreshToken { get; set; } = string.Empty;

    public DateTime FreshExpiryDate { get; set; }

    public Guid AccountId { get; set; }

    public string RoleName { get; set; } = string.Empty;
}
