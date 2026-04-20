using MediatR;

namespace Automotive.Marketplace.Application.Features.TransmissionFeatures.GetAllTransmissions;

public sealed record GetAllTransmissionsQuery : IRequest<IEnumerable<GetAllTransmissionsResponse>>;
