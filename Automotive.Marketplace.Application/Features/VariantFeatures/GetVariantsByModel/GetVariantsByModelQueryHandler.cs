using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.VariantFeatures.GetVariantsByModel;

public class GetVariantsByModelQueryHandler(IRepository repository, IMapper mapper)
    : IRequestHandler<GetVariantsByModelQuery, IEnumerable<GetVariantsByModelResponse>>
{
    public async Task<IEnumerable<GetVariantsByModelResponse>> Handle(GetVariantsByModelQuery request, CancellationToken cancellationToken)
    {
        var variants = await repository
            .AsQueryable<Variant>()
            .Where(v => v.ModelId == request.ModelId)
            .Include(v => v.Fuel)
            .Include(v => v.Transmission)
            .Include(v => v.BodyType)
            .OrderBy(v => v.Id)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<GetVariantsByModelResponse>>(variants);
    }
}
