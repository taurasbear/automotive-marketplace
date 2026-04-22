namespace Automotive.Marketplace.Application.Features.ListingFeatures.SearchListings;

public sealed record SearchListingsResponse
{
    public Guid Id { get; set; }
    public string MakeName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public int Year { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public string City { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public string? FirstImageUrl { get; set; }
}
