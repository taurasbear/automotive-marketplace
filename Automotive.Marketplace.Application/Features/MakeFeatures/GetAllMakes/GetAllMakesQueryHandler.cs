using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.MakeFeatures.GetAllMakes;

public class GetAllMakesQueryHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<GetAllMakesQuery, IEnumerable<GetAllMakesResponse>>
{
    public async Task<IEnumerable<GetAllMakesResponse>> Handle(
        GetAllMakesQuery request,
        CancellationToken cancellationToken)
    {
        var makes = await repository
            .AsQueryable<Make>()
            .OrderBy(make => make.Name)
            .ToListAsync(cancellationToken);

        var response = mapper.Map<IEnumerable<GetAllMakesResponse>>(makes);

        return response;
    }
}
