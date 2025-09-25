using MediatR;

namespace Automotive.Marketplace.Application.Features.CarFeatures.DeleteCar;

public sealed record DeleteCarCommand : IRequest
{
    public Guid Id { get; set; }
};