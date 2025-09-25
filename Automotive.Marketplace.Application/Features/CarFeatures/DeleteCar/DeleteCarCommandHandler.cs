using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.CarFeatures.DeleteCar;

public class DeleteCarCommandHandler(IRepository repository) : IRequestHandler<DeleteCarCommand>
{
    public async Task Handle(DeleteCarCommand request, CancellationToken cancellationToken)
    {
        var car = await repository.GetByIdAsync<Car>(request.Id, cancellationToken);

        await repository.DeleteAsync(car, cancellationToken);
    }
}