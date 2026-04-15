using MediatR;

namespace Automotive.Marketplace.Application.Features.BodyTypeFeatures.GetAllBodyTypes;

public sealed record GetAllBodyTypesQuery : IRequest<IEnumerable<GetAllBodyTypesResponse>>;
