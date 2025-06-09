namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingDetailsWithCar;

public sealed record GetListingsDetailsWithCarResponse
{
    public IEnumerable<ListingDetailsWithCar> ListingsDetailsWithCar { get; set; } = new List<ListingDetailsWithCar>();

    public sealed record ListingDetailsWithCar
    {
        public bool Used { get; set; }

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
    }
}
