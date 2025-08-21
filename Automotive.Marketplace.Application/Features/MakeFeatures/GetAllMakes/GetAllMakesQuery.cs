using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;

public sealed record class GetAllMakesQuery : IRequest<IEnumerable<GetAllMakesResponse>>;
