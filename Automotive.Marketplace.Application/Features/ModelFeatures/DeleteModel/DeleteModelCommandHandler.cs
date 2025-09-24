using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ModelFeatures.DeleteModel;

public class DeleteModelCommandHandler(IRepository repository) : IRequestHandler<DeleteModelCommand>
{
    public async Task Handle(DeleteModelCommand request, CancellationToken cancellationToken)
    {
        var model = await repository.GetByIdAsync<Model>(request.Id, cancellationToken);

        await repository.DeleteAsync(model, cancellationToken);
    }
}