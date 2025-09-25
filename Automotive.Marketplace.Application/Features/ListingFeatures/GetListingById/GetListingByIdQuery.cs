using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingById;

public sealed record GetListingByIdQuery : IRequest<GetListingByIdResponse>
{
    public Guid Id { get; set; }
};