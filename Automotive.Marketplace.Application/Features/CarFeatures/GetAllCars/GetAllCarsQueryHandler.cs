
using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.CarFeatures.GetAllCars;

public class GetAllCarsQueryHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<GetAllCarsQuery, IEnumerable<GetAllCarsResponse>>
{
    public async Task<IEnumerable<GetAllCarsResponse>> Handle(GetAllCarsQuery request, CancellationToken cancellationToken)
    {
        var cars = await repository.GetAllAsync<Car>(cancellationToken);
        cars = [.. cars.OrderByDescending(car => car.Year)];

        var response = mapper.Map<IEnumerable<GetAllCarsResponse>>(cars);
        return response;
    }
}