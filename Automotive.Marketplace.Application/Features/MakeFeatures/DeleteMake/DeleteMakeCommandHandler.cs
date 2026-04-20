using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.MakeFeatures.DeleteMake;

public class DeleteMakeCommandHandler(IRepository repository) : IRequestHandler<DeleteMakeCommand>
{
    public async Task Handle(DeleteMakeCommand request, CancellationToken cancellationToken)
    {
        var make = await repository.GetByIdAsync<Make>(request.Id, cancellationToken);
        await repository.DeleteAsync(make, cancellationToken);
    }
}
