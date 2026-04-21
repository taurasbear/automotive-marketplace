using MediatR;

namespace Automotive.Marketplace.Application.Features.VariantFeatures.UpdateVariant;

public sealed record UpdateVariantCommand(
    Guid Id,
    Guid ModelId,
    Guid FuelId,
    Guid TransmissionId,
    Guid BodyTypeId,
    bool IsCustom,
    int DoorCount,
    int PowerKw,
    int EngineSizeMl
) : IRequest<UpdateVariantResponse>;
