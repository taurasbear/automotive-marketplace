using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.MakeFeatures.UpdateMake;

public class UpdateMakeCommandHandler(
    IMapper mapper,
    IRepository repository) : IRequestHandler<UpdateMakeCommand>
{
    public async Task Handle(UpdateMakeCommand request, CancellationToken cancellationToken)
    {
        var make = await repository.GetByIdAsync<Make>(request.Id, cancellationToken);
        mapper.Map(request, make);
        await repository.UpdateAsync(make, cancellationToken);
    }
}
