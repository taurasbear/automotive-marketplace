namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;

public sealed record GetAllListingsResponse
{
    public Guid Id { get; set; }

    public bool IsUsed { get; set; }

    public string Year { get; set; } = string.Empty;

    public string Make { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public int Mileage { get; set; }

    public decimal Price { get; set; }

    public int EngineSize { get; set; }

    public int Power { get; set; }

    public string FuelType { get; set; } = string.Empty;

    public string Transmission { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public List<Image> Images { get; set; } = [];

    public sealed class Image
    {
        public string Url { get; set; } = string.Empty;

        public string AltText { get; set; } = string.Empty;
    }
}
