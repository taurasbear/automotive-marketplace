using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.CreateListing;

public class CreateListingCommandHandler(
    IMapper mapper,
    IRepository repository,
    IImageStorageService imageStorageService) : IRequestHandler<CreateListingCommand>
{
    public async Task Handle(CreateListingCommand request, CancellationToken cancellationToken)
    {
        var car = mapper.Map<Car>(request);
        var listing = mapper.Map<Listing>(request);

        listing.Car = car;

        await repository.CreateAsync(listing, cancellationToken);

        foreach (var imageFile in request.Images)
        {
            var uniqueFileName = $"{Guid.NewGuid()}_{imageFile.FileName}";
            var imageUploadResult = await imageStorageService.UploadImageAsync(imageFile, uniqueFileName);

            var model = await repository.GetByIdAsync<Model>(car.ModelId, cancellationToken);
            var make = model.Make;
            var image = new Image
            {
                ListingId = listing.Id,
                BucketName = imageUploadResult.BucketName,
                ObjectKey = imageUploadResult.ObjectKey,
                OriginalFileName = imageFile.FileName,
                ContentType = imageFile.ContentType,
                FileSize = imageUploadResult.FileSize,
                AltText = $"{car.Year.Year} {make.Name} {model.Name}"
            };

            await repository.CreateAsync(image, cancellationToken);
        }

        return;
    }
}