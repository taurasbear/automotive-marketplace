using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.BodyTypeFeatures.GetAllBodyTypes;

public class GetAllBodyTypesQueryHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<GetAllBodyTypesQuery, IEnumerable<GetAllBodyTypesResponse>>
{
    public async Task<IEnumerable<GetAllBodyTypesResponse>> Handle(GetAllBodyTypesQuery request, CancellationToken cancellationToken)
    {
        var bodyTypes = await repository
            .AsQueryable<BodyType>()
            .Include(b => b.Translations)
            .OrderBy(b => b.Name)
            .ToListAsync(cancellationToken);

        return mapper.Map<IEnumerable<GetAllBodyTypesResponse>>(bodyTypes);
    }
}
