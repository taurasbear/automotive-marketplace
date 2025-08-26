namespace Automotive.Marketplace.Application.Features.AuthFeatures.LoginUser;

public sealed record LoginUserResponse
{
    public string FreshAccessToken { get; set; } = string.Empty;

    public string FreshRefreshToken { get; set; } = string.Empty;

    public DateTime FreshExpiryDate { get; set; }

    public Guid UserId { get; set; }

    public string RoleName { get; set; } = string.Empty;
}
