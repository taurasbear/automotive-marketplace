using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetUserContractProfile;

public sealed record GetUserContractProfileQuery : IRequest<GetUserContractProfileResponse>
{
    public Guid UserId { get; set; }
}
