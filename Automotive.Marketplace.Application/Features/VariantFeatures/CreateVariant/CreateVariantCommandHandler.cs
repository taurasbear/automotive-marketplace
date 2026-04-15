using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using AutoMapper;
using MediatR;

namespace Automotive.Marketplace.Application.Features.VariantFeatures.CreateVariant;

public class CreateVariantCommandHandler(IRepository repository, IMapper mapper)
    : IRequestHandler<CreateVariantCommand, CreateVariantResponse>
{
    public async Task<CreateVariantResponse> Handle(CreateVariantCommand request, CancellationToken cancellationToken)
    {
        var variant = mapper.Map<Variant>(request);
        variant.Id = Guid.NewGuid();

        await repository.CreateAsync(variant, cancellationToken);

        return mapper.Map<CreateVariantResponse>(variant);
    }
}
