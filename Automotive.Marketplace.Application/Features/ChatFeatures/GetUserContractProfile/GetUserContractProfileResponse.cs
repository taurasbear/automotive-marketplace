namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetUserContractProfile;

public sealed record GetUserContractProfileResponse
{
    public string? PhoneNumber { get; set; }
    public string? PersonalIdCode { get; set; }
    public string? Address { get; set; }
}
