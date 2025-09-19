using AutoMapper;
using Automotive.Marketplace.Domain.Enums;
using MediatR;

namespace Automotive.Marketplace.Application.Features.EnumFeatures.GetTransmissionTypes;

public class GetTransmissionTypesQueryHandler(
    IMapper mapper) : IRequestHandler<GetTransmissionTypesQuery, IEnumerable<GetTransmissionTypesResponse>>
{
    public Task<IEnumerable<GetTransmissionTypesResponse>> Handle(
        GetTransmissionTypesQuery request,
        CancellationToken cancellationToken)
    {
        var transmissionTypes = Enum.GetValues(typeof(Transmission)).Cast<Transmission>();
        var response = mapper.Map<IEnumerable<GetTransmissionTypesResponse>>(transmissionTypes);

        return Task.FromResult(response);
    }
}
