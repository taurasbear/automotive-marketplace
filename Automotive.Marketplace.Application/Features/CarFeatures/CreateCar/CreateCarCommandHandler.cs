
using AutoMapper;
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.CarFeatures.CreateCar;

public class CreateCarCommandHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<CreateCarCommand>
{
    public async Task Handle(CreateCarCommand request, CancellationToken cancellationToken)
    {
        if (!repository.AsQueryable<Model>().Where(model => model.Id == request.ModelId).Any())
        {
            throw new DbEntityNotFoundException(typeof(Model).Name, request.ModelId);
        }

        var car = mapper.Map<Car>(request);

        await repository.CreateAsync(car, cancellationToken);
    }
}