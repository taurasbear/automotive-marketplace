using MediatR;

namespace Automotive.Marketplace.Application.Features.CarFeatures.GetAllCars;

public sealed record GetAllCarsQuery : IRequest<IEnumerable<GetAllCarsResponse>>;