namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;

public sealed record GetAllMakesResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}
