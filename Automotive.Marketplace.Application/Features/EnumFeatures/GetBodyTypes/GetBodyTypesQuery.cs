using MediatR;

namespace Automotive.Marketplace.Application.Features.EnumFeatures.GetBodyTypes;

public sealed record class GetBodyTypesQuery : IRequest<IEnumerable<GetBodyTypesResponse>>;
