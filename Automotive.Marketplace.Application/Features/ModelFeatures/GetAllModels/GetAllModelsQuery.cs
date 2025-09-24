using MediatR;

namespace Automotive.Marketplace.Application.Features.ModelFeatures.GetAllModels;

public sealed record GetAllModelsQuery : IRequest<IEnumerable<GetAllModelsResponse>>;