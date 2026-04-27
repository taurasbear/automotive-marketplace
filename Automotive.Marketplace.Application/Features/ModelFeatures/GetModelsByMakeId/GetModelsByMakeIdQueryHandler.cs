
using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ModelFeatures.GetModelsByMakeId;

public class GetModelsByMakeIdQueryHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<GetModelsByMakeIdQuery, IEnumerable<GetModelsByMakeIdResponse>>
{
    public async Task<IEnumerable<GetModelsByMakeIdResponse>> Handle(GetModelsByMakeIdQuery request, CancellationToken cancellationToken)
    {
        var query = repository
            .AsQueryable<Model>()
            .Where(model => model.MakeId == request.MakeId);

        if (request.OnlyWithListings)
        {
            query = query.Where(model =>
                model.Variants.Any(v => v.Listings.Any(l => l.Status == Status.Available)));
        }

        var models = await query
            .OrderBy(model => model.Name)
            .ToListAsync(cancellationToken);

        var response = mapper.Map<IEnumerable<GetModelsByMakeIdResponse>>(models);
        return response;
    }
}