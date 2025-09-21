using MediatR;

namespace Automotive.Marketplace.Application.Features.MakeFeatures.GetAllMakes;

public sealed record class GetAllMakesQuery : IRequest<IEnumerable<GetAllMakesResponse>>;
