using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.CarFeatures.UpdateCar;

public class UpdateCarCommandHandler(
    IRepository repository,
    IMapper mapper) : IRequestHandler<UpdateCarCommand>
{
    public async Task Handle(UpdateCarCommand request, CancellationToken cancellationToken)
    {
        var car = mapper.Map<Car>(request);

        await repository.UpdateAsync(car, cancellationToken);
    }
}