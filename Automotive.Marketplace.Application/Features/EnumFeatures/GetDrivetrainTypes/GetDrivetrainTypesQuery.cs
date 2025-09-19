using MediatR;

namespace Automotive.Marketplace.Application.Features.EnumFeatures.GetDrivetrainTypes;

public sealed record class GetDrivetrainTypesQuery : IRequest<IEnumerable<GetDrivetrainTypesResponse>>;
