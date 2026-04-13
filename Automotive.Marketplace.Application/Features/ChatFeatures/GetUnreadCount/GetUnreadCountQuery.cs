using MediatR;

namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetUnreadCount;

public sealed record GetUnreadCountQuery : IRequest<GetUnreadCountResponse>
{
    public Guid UserId { get; set; }
}
