using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;

public sealed record class GetAllListingsQuery : IRequest<IEnumerable<GetAllListingsResponse>>;
