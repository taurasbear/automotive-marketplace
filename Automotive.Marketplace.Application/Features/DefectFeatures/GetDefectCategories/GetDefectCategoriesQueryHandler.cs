using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.DefectFeatures.GetDefectCategories;

public class GetDefectCategoriesQueryHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<GetDefectCategoriesQuery, IEnumerable<GetDefectCategoriesResponse>>
{
    public async Task<IEnumerable<GetDefectCategoriesResponse>> Handle(GetDefectCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await repository
            .AsQueryable<DefectCategory>()
            .Include(dc => dc.Translations)
            .OrderBy(dc => dc.Name)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<GetDefectCategoriesResponse>>(categories);
    }
}
