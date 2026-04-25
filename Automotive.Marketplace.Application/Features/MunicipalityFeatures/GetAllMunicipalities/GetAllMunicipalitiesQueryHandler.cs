using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.MunicipalityFeatures.GetAllMunicipalities;

public class GetAllMunicipalitiesQueryHandler(IRepository repository)
    : IRequestHandler<GetAllMunicipalitiesQuery, IEnumerable<GetAllMunicipalitiesResponse>>
{
    public async Task<IEnumerable<GetAllMunicipalitiesResponse>> Handle(
        GetAllMunicipalitiesQuery request, CancellationToken cancellationToken)
    {
        return await repository
            .AsQueryable<Municipality>()
            .OrderBy(m => m.Name)
            .Select(m => new GetAllMunicipalitiesResponse { Id = m.Id, Name = m.Name })
            .ToListAsync(cancellationToken);
    }
}
