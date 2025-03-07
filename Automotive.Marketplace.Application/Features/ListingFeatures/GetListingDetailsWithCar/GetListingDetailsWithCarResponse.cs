namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingDetailsWithCar
{
    using Automotive.Marketplace.Domain.Entities;

    public sealed record class GetListingDetailsWithCarResponse
    {
        public IEnumerable<Listing> ListingDetailsWithCar { get; set; } = new List<Listing>();
    }
}
