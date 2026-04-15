using MediatR;

namespace Automotive.Marketplace.Application.Features.FuelFeatures.GetAllFuels;

public sealed record GetAllFuelsQuery : IRequest<IEnumerable<GetAllFuelsResponse>>;
