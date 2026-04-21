using MediatR;

namespace Automotive.Marketplace.Application.Features.VariantFeatures.CreateVariant;

public sealed record CreateVariantCommand(
    Guid ModelId,
    Guid FuelId,
    Guid TransmissionId,
    Guid BodyTypeId,
    bool IsCustom,
    int DoorCount,
    int PowerKw,
    int EngineSizeMl
) : IRequest<CreateVariantResponse>;
