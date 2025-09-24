
using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ModelFeatures.GetModelById;

public class GetModelByIdQueryHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<GetModelByIdQuery, GetModelByIdResponse>
{
    public async Task<GetModelByIdResponse> Handle(GetModelByIdQuery request, CancellationToken cancellationToken)
    {
        var model = await repository.GetByIdAsync<Model>(request.Id, cancellationToken);

        var response = mapper.Map<GetModelByIdResponse>(model);
        return response;
    }
}