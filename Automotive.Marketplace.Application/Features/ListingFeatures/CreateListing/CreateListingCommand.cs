using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.CreateListing;

public sealed record CreateListingCommand(
    decimal Price,
    int Mileage,
    string Description,
    Guid SellerId,
    Guid? VariantId,
    Guid ModelId,
    int Year,
    Guid FuelId,
    Guid TransmissionId,
    Guid BodyTypeId,
    Guid DrivetrainId,
    bool IsCustom,
    int DoorCount,
    int PowerKw,
    int EngineSizeMl,
    bool IsUsed,
    string City
) : IRequest<CreateListingResponse>;