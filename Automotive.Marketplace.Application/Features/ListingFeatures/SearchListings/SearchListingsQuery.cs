using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.SearchListings;

public sealed record SearchListingsQuery : IRequest<IEnumerable<SearchListingsResponse>>
{
    public string Q { get; set; } = string.Empty;
    public int Limit { get; set; } = 10;
}
