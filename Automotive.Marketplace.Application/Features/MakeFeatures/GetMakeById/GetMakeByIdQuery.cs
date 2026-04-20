using MediatR;

namespace Automotive.Marketplace.Application.Features.MakeFeatures.GetMakeById;

public sealed record GetMakeByIdQuery : IRequest<GetMakeByIdResponse>
{
    public Guid Id { get; set; }
}
