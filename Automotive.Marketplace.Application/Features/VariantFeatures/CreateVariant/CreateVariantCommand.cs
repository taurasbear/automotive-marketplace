using MediatR;

namespace Automotive.Marketplace.Application.Features.VariantFeatures.CreateVariant;

public sealed record CreateVariantCommand(
    Guid ModelId,
    int Year,
    Guid FuelId,
    Guid TransmissionId,
    Guid BodyTypeId,
    bool IsCustom,
    int DoorCount,
    int PowerKw,
    int EngineSizeMl
) : IRequest<CreateVariantResponse>;
