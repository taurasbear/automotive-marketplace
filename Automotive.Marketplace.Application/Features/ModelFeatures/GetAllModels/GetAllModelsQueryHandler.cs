
using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ModelFeatures.GetAllModels;

public class GetAllModelsQueryHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<GetAllModelsQuery, IEnumerable<GetAllModelsResponse>>
{
    public async Task<IEnumerable<GetAllModelsResponse>> Handle(GetAllModelsQuery request, CancellationToken cancellationToken)
    {
        var models = await repository.GetAllAsync<Model>(cancellationToken);
        models = [.. models.OrderByDescending(model => model.FirstAppearanceDate)];

        var response = mapper.Map<IEnumerable<GetAllModelsResponse>>(models);
        return response;
    }
}