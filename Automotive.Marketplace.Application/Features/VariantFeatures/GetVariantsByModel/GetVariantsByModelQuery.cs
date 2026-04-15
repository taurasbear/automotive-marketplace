using MediatR;

namespace Automotive.Marketplace.Application.Features.VariantFeatures.GetVariantsByModel;

public sealed record GetVariantsByModelQuery(Guid ModelId) : IRequest<IEnumerable<GetVariantsByModelResponse>>;
