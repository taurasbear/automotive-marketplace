using AutoMapper;
using Automotive.Marketplace.Application.Common.Exceptions;
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
        if (!repository.AsQueryable<Make>().Where(make => make.Id == request.MakeId).Any())
        {
            throw new DbEntityNotFoundException(typeof(Make).Name, request.MakeId);
        }

        if (!repository.AsQueryable<Model>().Where(model => model.Id == request.Id).Any())
        {
            throw new DbEntityNotFoundException(typeof(Model).Name, request.Id);
        }

        var model = mapper.Map<Model>(request);

        await repository.UpdateAsync(model, cancellationToken);
    }
}