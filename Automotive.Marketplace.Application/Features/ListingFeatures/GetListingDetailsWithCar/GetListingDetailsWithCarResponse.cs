namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingDetailsWithCar
{
    using Automotive.Marketplace.Domain.Entities;

    public sealed record GetListingDetailsWithCarResponse
    {
        public IEnumerable<GetListingWithCarResponse> ListingDetailsWithCar { get; set; } = new List<GetListingWithCarResponse>();

        public sealed record GetListingWithCarResponse
        {
            public decimal Price { get; set; }

            public string Description { get; set; } = string.Empty;

            public string City { get; set; } = string.Empty;

            public int Mileage { get; set; }

            public int Power { get; set; }

            public int EngineSize { get; set; }

            public DateTime Year { get; set; }

            public string ModelName { get; set; } = string.Empty;
        }
    }
}
