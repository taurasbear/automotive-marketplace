using MediatR;

namespace Automotive.Marketplace.Application.Features.ModelFeatures.GetModelById;

public sealed record GetModelByIdQuery : IRequest<GetModelByIdResponse>
{
    public required Guid Id { get; set; }
};