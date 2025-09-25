
using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.CarFeatures.GetCarById;

public class GetCarByIdQueryHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<GetCarByIdQuery, GetCarByIdResponse>
{
    public async Task<GetCarByIdResponse> Handle(GetCarByIdQuery request, CancellationToken cancellationToken)
    {
        var car = await repository.GetByIdAsync<Car>(request.Id, cancellationToken);

        var response = mapper.Map<GetCarByIdResponse>(car);
        return response;
    }
}