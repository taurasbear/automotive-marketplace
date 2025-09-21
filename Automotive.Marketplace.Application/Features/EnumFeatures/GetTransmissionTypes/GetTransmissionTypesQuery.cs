using MediatR;

namespace Automotive.Marketplace.Application.Features.EnumFeatures.GetTransmissionTypes;

public sealed record class GetTransmissionTypesQuery : IRequest<IEnumerable<GetTransmissionTypesResponse>>;
