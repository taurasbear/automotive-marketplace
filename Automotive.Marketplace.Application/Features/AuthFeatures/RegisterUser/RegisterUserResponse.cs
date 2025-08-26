namespace Automotive.Marketplace.Application.Features.AuthFeatures.RegisterUser;

public sealed record RegisterUserResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime RefreshTokenExpiryDate { get; set; }

    public Guid UserId { get; set; }
}
