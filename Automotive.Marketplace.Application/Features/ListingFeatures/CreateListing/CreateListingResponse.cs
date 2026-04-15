namespace Automotive.Marketplace.Application.Features.ListingFeatures.CreateListing;

public sealed record CreateListingResponse
{
    public Guid Id { get; set; }
    public decimal Price { get; set; }
    public int Mileage { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid SellerId { get; set; }
    public Guid VariantId { get; set; }
    public Guid DrivetrainId { get; set; }
}
