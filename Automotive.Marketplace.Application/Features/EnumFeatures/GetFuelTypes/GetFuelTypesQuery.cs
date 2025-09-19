using MediatR;

namespace Automotive.Marketplace.Application.Features.EnumFeatures.GetFuelTypes;

public sealed record class GetFuelTypesQuery : IRequest<IEnumerable<GetFuelTypesResponse>>;
