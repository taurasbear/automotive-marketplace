using Automotive.Marketplace.Application.Models;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetMyListings;

public sealed record GetMyListingsResponse
{
    public Guid Id { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public bool IsUsed { get; set; }
    public string City { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int Year { get; set; }
    public string MakeName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public ImageDto? Thumbnail { get; set; }
    public int ImageCount { get; set; }
    public int DefectCount { get; set; }
}