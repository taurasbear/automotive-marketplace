using Automotive.Marketplace.Application.Models;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;

public sealed record GetAllListingsResponse
{
    public Guid Id { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool IsUsed { get; set; }
    public string City { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public Guid VariantId { get; set; }
    public int Year { get; set; }
    public string MakeName { get; set; } = string.Empty;
    public string ModelName { get; set; } = string.Empty;
    public string FuelName { get; set; } = string.Empty;
    public string TransmissionName { get; set; } = string.Empty;
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public int PowerKw { get; set; }
    public int EngineSizeMl { get; set; }
    public ImageDto? Thumbnail { get; set; }
    public bool IsLiked { get; set; }
}
