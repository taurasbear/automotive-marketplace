using AutoMapper;
using Automotive.Marketplace.Domain.Enums;
using MediatR;

namespace Automotive.Marketplace.Application.Features.EnumFeatures.GetFuelTypes;

public class GetFuelTypesQueryHandler(
    IMapper mapper) : IRequestHandler<GetFuelTypesQuery, IEnumerable<GetFuelTypesResponse>>
{
    public Task<IEnumerable<GetFuelTypesResponse>> Handle(
        GetFuelTypesQuery request,
        CancellationToken cancellationToken)
    {
        var fuelTypes = Enum.GetValues(typeof(Fuel)).Cast<Fuel>();
        var response = mapper.Map<IEnumerable<GetFuelTypesResponse>>(fuelTypes);

        return Task.FromResult(response);
    }
}
