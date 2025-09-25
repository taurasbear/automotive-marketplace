namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingById;

public sealed record GetListingByIdResponse
{
    public string Make { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public string? Colour { get; set; }

    public string? Vin { get; set; }

    public int Power { get; set; }

    public int EngineSize { get; set; }

    public int Mileage { get; set; }

    public bool IsSteeringWheelRight { get; set; }

    public string City { get; set; } = string.Empty;

    public bool IsUsed { get; set; }

    public int Year { get; set; }

    public string Transmission { get; set; } = string.Empty;

    public string Fuel { get; set; } = string.Empty;

    public int DoorCount { get; set; }

    public string BodyType { get; set; } = string.Empty;

    public string Drivetrain { get; set; } = string.Empty;

    public string Seller { get; set; } = string.Empty;

    public List<Image> Images { get; set; } = [];

    public sealed class Image
    {
        public string Url { get; set; } = string.Empty;

        public string AltText { get; set; } = string.Empty;
    }
}