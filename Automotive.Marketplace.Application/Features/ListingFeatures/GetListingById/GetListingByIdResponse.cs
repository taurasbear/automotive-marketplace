namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingById;

public sealed record GetListingByIdResponse
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
    public string BodyTypeName { get; set; } = string.Empty;
    public string DrivetrainName { get; set; } = string.Empty;
    public int DoorCount { get; set; }
    public int PowerKw { get; set; }
    public int EngineSizeMl { get; set; }
    public Guid SellerId { get; set; }
    public string SellerName { get; set; } = string.Empty;
    public IEnumerable<Automotive.Marketplace.Application.Models.ImageDto> Images { get; set; } = [];
}