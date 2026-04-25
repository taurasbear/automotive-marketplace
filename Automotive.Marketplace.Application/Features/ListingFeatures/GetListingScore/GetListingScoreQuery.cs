using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingScore;

public class GetListingScoreQuery : IRequest<GetListingScoreResponse>
{
    public Guid ListingId { get; set; }
    public Guid? UserId { get; set; }
}
