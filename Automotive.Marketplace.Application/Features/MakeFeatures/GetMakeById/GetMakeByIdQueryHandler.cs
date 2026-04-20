using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.MakeFeatures.GetMakeById;

public class GetMakeByIdQueryHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<GetMakeByIdQuery, GetMakeByIdResponse>
{
    public async Task<GetMakeByIdResponse> Handle(GetMakeByIdQuery request, CancellationToken cancellationToken)
    {
        var make = await repository.GetByIdAsync<Make>(request.Id, cancellationToken);
        return mapper.Map<GetMakeByIdResponse>(make);
    }
}
