namespace Automotive.Marketplace.Application.Features.AuthFeatures.RefreshToken
{

    public sealed record class RefreshTokenResponse
    {
        public string FreshRefreshToken { get; set; } = string.Empty;

        public string FreshAccessToken { get; set; } = string.Empty;

        public DateTime FreshExpiryDate { get; set; }
    }
}
