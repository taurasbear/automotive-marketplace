using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.FuelFeatures.GetAllFuels;

public class GetAllFuelsQueryHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<GetAllFuelsQuery, IEnumerable<GetAllFuelsResponse>>
{
    public async Task<IEnumerable<GetAllFuelsResponse>> Handle(GetAllFuelsQuery request, CancellationToken cancellationToken)
    {
        var fuels = await repository
            .AsQueryable<Fuel>()
            .Include(f => f.Translations)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<GetAllFuelsResponse>>(fuels);
    }
}
