using MediatR;
using Microsoft.AspNetCore.Http;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.CreateListing;

public sealed record CreateListingCommand(
    decimal Price,
    int Mileage,
    string? Description,
    string? Colour,
    string? Vin,
    bool IsSteeringWheelRight,
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
    Guid MunicipalityId,
    List<IFormFile> Images
) : IRequest<CreateListingResponse>;