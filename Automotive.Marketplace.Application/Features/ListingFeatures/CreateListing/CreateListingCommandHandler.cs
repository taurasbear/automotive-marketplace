using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.CreateListing;

public class CreateListingCommandHandler(IRepository repository, IMapper mapper, IImageStorageService imageStorageService)
    : IRequestHandler<CreateListingCommand, CreateListingResponse>
{
    public async Task<CreateListingResponse> Handle(CreateListingCommand request, CancellationToken cancellationToken)
    {
        Guid variantId;

        if (request.VariantId.HasValue)
        {
            await repository.GetByIdAsync<Variant>(request.VariantId.Value, cancellationToken);
            variantId = request.VariantId.Value;
        }
        else if (request.IsCustom)
        {
            var customVariant = new Variant
            {
                Id = Guid.NewGuid(),
                ModelId = request.ModelId,
                FuelId = request.FuelId,
                TransmissionId = request.TransmissionId,
                BodyTypeId = request.BodyTypeId,
                IsCustom = true,
                DoorCount = request.DoorCount,
                PowerKw = request.PowerKw,
                EngineSizeMl = request.EngineSizeMl,
            };
            await repository.CreateAsync(customVariant, cancellationToken);
            variantId = customVariant.Id;
        }
        else
        {
            var existing = await repository
                .AsQueryable<Variant>()
                .FirstOrDefaultAsync(v =>
                    v.ModelId == request.ModelId &&
                    v.FuelId == request.FuelId &&
                    v.TransmissionId == request.TransmissionId &&
                    v.BodyTypeId == request.BodyTypeId &&
                    !v.IsCustom,
                    cancellationToken);

            if (existing != null)
            {
                variantId = existing.Id;
            }
            else
            {
                var newVariant = new Variant
                {
                    Id = Guid.NewGuid(),
                    ModelId = request.ModelId,
                    FuelId = request.FuelId,
                    TransmissionId = request.TransmissionId,
                    BodyTypeId = request.BodyTypeId,
                    IsCustom = false,
                    DoorCount = request.DoorCount,
                    PowerKw = request.PowerKw,
                    EngineSizeMl = request.EngineSizeMl,
                };
                await repository.CreateAsync(newVariant, cancellationToken);
                variantId = newVariant.Id;
            }
        }

        var listing = new Listing
        {
            Id = Guid.NewGuid(),
            Price = request.Price,
            Year = request.Year,
            Mileage = request.Mileage,
            Description = request.Description,
            Vin = request.Vin,
            Colour = request.Colour,
            IsSteeringWheelRight = request.IsSteeringWheelRight,
            SellerId = request.SellerId,
            VariantId = variantId,
            DrivetrainId = request.DrivetrainId,
            IsUsed = request.IsUsed,
            MunicipalityId = request.MunicipalityId,
            Status = Status.Available,
        };

        await repository.CreateAsync(listing, cancellationToken);

        var model = await repository
            .AsQueryable<Model>()
            .Include(m => m.Make)
            .FirstOrDefaultAsync(m => m.Id == request.ModelId, cancellationToken);

        var altText = model != null
            ? $"{request.Year} {model.Make.Name} {model.Name}"
            : $"{request.Year} listing";

        foreach (var imageFile in request.Images)
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
            var uploadResult = await imageStorageService.UploadImageAsync(imageFile, uniqueFileName);

            var image = new Image
            {
                Id = Guid.NewGuid(),
                ListingId = listing.Id,
                BucketName = uploadResult.BucketName,
                ObjectKey = uploadResult.ObjectKey,
                OriginalFileName = imageFile.FileName,
                ContentType = imageFile.ContentType,
                FileSize = uploadResult.FileSize,
                AltText = altText,
            };

            await repository.CreateAsync(image, cancellationToken);
        }

        return mapper.Map<CreateListingResponse>(listing);
    }
}
