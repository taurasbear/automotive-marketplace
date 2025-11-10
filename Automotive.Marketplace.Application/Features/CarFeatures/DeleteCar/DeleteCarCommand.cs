using MediatR;

namespace Automotive.Marketplace.Application.Features.CarFeatures.DeleteCar;

public sealed record DeleteCarCommand : IRequest
{
    public required Guid Id { get; set; }
};