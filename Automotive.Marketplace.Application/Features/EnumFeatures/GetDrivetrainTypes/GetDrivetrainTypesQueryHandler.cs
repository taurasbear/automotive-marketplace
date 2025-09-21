using AutoMapper;
using Automotive.Marketplace.Domain.Enums;
using MediatR;

namespace Automotive.Marketplace.Application.Features.EnumFeatures.GetDrivetrainTypes;

public class GetDrivetrainTypesQueryHandler(
    IMapper mapper) : IRequestHandler<GetDrivetrainTypesQuery, IEnumerable<GetDrivetrainTypesResponse>>
{
    public Task<IEnumerable<GetDrivetrainTypesResponse>> Handle(
        GetDrivetrainTypesQuery request,
        CancellationToken cancellationToken)
    {
        var drivetrainTypes = Enum.GetValues(typeof(Drivetrain)).Cast<Drivetrain>();
        var response = mapper.Map<IEnumerable<GetDrivetrainTypesResponse>>(drivetrainTypes);

        return Task.FromResult(response);
    }
}
