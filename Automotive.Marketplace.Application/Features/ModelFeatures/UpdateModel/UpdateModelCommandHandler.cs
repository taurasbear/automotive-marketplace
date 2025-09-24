using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ModelFeatures.UpdateModel;

public class UpdateModelCommandHandler(
    IRepository repository,
    IMapper mapper) : IRequestHandler<UpdateModelCommand>
{
    public async Task Handle(UpdateModelCommand request, CancellationToken cancellationToken)
    {
        var model = mapper.Map<Model>(request);

        await repository.UpdateAsync(model, cancellationToken);
    }
}