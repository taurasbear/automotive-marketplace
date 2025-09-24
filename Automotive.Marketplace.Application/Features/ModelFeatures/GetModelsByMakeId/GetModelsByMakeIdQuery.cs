using MediatR;

namespace Automotive.Marketplace.Application.Features.ModelFeatures.GetModelsByMakeId;

public sealed record GetModelsByMakeIdQuery : IRequest<IEnumerable<GetModelsByMakeIdResponse>>
{
    public Guid MakeId { get; set; }
};