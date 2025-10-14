
using AutoMapper;
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ModelFeatures.CreateModel;

public class CreateModelCommandHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<CreateModelCommand>
{
    public async Task Handle(CreateModelCommand request, CancellationToken cancellationToken)
    {
        if (!repository.AsQueryable<Make>().Where(make => make.Id == request.MakeId).Any())
        {
            throw new DbEntityNotFoundException(typeof(Make).Name, request.MakeId);
        }

        var model = mapper.Map<Model>(request);

        await repository.CreateAsync(model, cancellationToken);
    }
}