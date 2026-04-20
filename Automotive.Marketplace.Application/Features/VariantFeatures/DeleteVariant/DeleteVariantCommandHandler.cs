using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.VariantFeatures.DeleteVariant;

public class DeleteVariantCommandHandler(IRepository repository)
    : IRequestHandler<DeleteVariantCommand>
{
    public async Task Handle(DeleteVariantCommand request, CancellationToken cancellationToken)
    {
        var variant = await repository.GetByIdAsync<Variant>(request.Id, cancellationToken);

        await repository.DeleteAsync(variant, cancellationToken);
    }
}
