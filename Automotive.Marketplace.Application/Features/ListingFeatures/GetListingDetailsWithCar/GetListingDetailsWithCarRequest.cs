namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingDetailsWithCar;

using MediatR;

public sealed record class GetListingDetailsWithCarRequest() : IRequest<GetListingsDetailsWithCarResponse>;
