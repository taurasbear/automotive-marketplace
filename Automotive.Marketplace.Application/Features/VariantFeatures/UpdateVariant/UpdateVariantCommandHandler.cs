using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using AutoMapper;
using MediatR;

namespace Automotive.Marketplace.Application.Features.VariantFeatures.UpdateVariant;

public class UpdateVariantCommandHandler(IRepository repository, IMapper mapper)
    : IRequestHandler<UpdateVariantCommand, UpdateVariantResponse>
{
    public async Task<UpdateVariantResponse> Handle(UpdateVariantCommand request, CancellationToken cancellationToken)
    {
        var variant = await repository.GetByIdAsync<Variant>(request.Id, cancellationToken);

        variant.ModelId = request.ModelId;
        variant.FuelId = request.FuelId;
        variant.TransmissionId = request.TransmissionId;
        variant.BodyTypeId = request.BodyTypeId;
        variant.IsCustom = request.IsCustom;
        variant.DoorCount = request.DoorCount;
        variant.PowerKw = request.PowerKw;
        variant.EngineSizeMl = request.EngineSizeMl;

        await repository.UpdateAsync(variant, cancellationToken);

        return mapper.Map<UpdateVariantResponse>(variant);
    }
}
