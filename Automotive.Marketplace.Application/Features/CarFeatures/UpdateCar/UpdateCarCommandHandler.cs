using AutoMapper;
using Automotive.Marketplace.Application.Common.Exceptions;
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
        if (!repository.AsQueryable<Model>().Where(model => model.Id == request.ModelId).Any())
        {
            throw new DbEntityNotFoundException(typeof(Model).Name, request.ModelId);
        }
        if (!repository.AsQueryable<Car>().Where(car => car.Id == request.Id).Any())
        {
            throw new DbEntityNotFoundException(typeof(Car).Name, request.Id);
        }

        var car = mapper.Map<Car>(request);

        await repository.UpdateAsync(car, cancellationToken);
    }
}