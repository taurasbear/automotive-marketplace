using MediatR;

namespace Automotive.Marketplace.Application.Features.CarFeatures.GetCarById;

public sealed record GetCarByIdQuery : IRequest<GetCarByIdResponse>
{
    public required Guid Id { get; set; }
};