using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.CancelAvailability;

public sealed record CancelAvailabilityCommand : IRequest<CancelAvailabilityResponse>
{
    public Guid AvailabilityCardId { get; set; }

    public Guid CancellerId { get; set; }
}
