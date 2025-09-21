using AutoMapper;
using Automotive.Marketplace.Domain.Enums;
using MediatR;

namespace Automotive.Marketplace.Application.Features.EnumFeatures.GetBodyTypes;

public class GetBodyTypesQueryHandler(
    IMapper mapper) : IRequestHandler<GetBodyTypesQuery, IEnumerable<GetBodyTypesResponse>>
{
    public Task<IEnumerable<GetBodyTypesResponse>> Handle(
        GetBodyTypesQuery request,
        CancellationToken cancellationToken)
    {
        var bodyTypes = Enum.GetValues(typeof(BodyType)).Cast<BodyType>();
        var response = mapper.Map<IEnumerable<GetBodyTypesResponse>>(bodyTypes);

        return Task.FromResult(response);
    }
}
