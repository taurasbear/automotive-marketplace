using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.TransmissionFeatures.GetAllTransmissions;

public class GetAllTransmissionsQueryHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<GetAllTransmissionsQuery, IEnumerable<GetAllTransmissionsResponse>>
{
    public async Task<IEnumerable<GetAllTransmissionsResponse>> Handle(GetAllTransmissionsQuery request, CancellationToken cancellationToken)
    {
        var transmissions = await repository
            .AsQueryable<Transmission>()
            .Include(t => t.Translations)
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<GetAllTransmissionsResponse>>(transmissions);
    }
}
