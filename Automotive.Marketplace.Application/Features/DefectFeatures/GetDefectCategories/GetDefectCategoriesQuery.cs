using MediatR;

namespace Automotive.Marketplace.Application.Features.DefectFeatures.GetDefectCategories;

public sealed record GetDefectCategoriesQuery : IRequest<IEnumerable<GetDefectCategoriesResponse>>;
