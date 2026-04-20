using MediatR;

namespace Automotive.Marketplace.Application.Features.VariantFeatures.UpdateVariant;

public sealed record UpdateVariantCommand(
    Guid Id,
    Guid ModelId,
    int Year,
    Guid FuelId,
    Guid TransmissionId,
    Guid BodyTypeId,
    bool IsCustom,
    int DoorCount,
    int PowerKw,
    int EngineSizeMl
) : IRequest<UpdateVariantResponse>;
