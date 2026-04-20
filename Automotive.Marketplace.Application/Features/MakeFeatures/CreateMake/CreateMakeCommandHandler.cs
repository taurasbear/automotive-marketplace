using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.MakeFeatures.CreateMake;

public class CreateMakeCommandHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<CreateMakeCommand>
{
    public async Task Handle(CreateMakeCommand request, CancellationToken cancellationToken)
    {
        var make = mapper.Map<Make>(request);
        await repository.CreateAsync(make, cancellationToken);
    }
}
