using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.DefectFeatures.AddDefectImage;

public class AddDefectImageCommandHandler(IRepository repository, IImageStorageService imageStorageService)
    : IRequestHandler<AddDefectImageCommand, Guid>
{
    public async Task<Guid> Handle(AddDefectImageCommand request, CancellationToken cancellationToken)
    {
        // Load defect with images
        var defect = await repository
            .AsQueryable<ListingDefect>()
            .Include(ld => ld.Images)
            .FirstOrDefaultAsync(ld => ld.Id == request.ListingDefectId, cancellationToken);

        if (defect == null)
        {
            throw new DbEntityNotFoundException(nameof(ListingDefect), request.ListingDefectId);
        }

        // Validate max 3 images
        if (defect.Images.Count >= 3)
        {
            throw new InvalidOperationException("Maximum of 3 images per defect allowed");
        }

        // Upload image to storage
        var uniqueFileName = $"{Guid.NewGuid()}_{request.Image.FileName}";
        var uploadResult = await imageStorageService.UploadImageAsync(request.Image, uniqueFileName);

        // Create Image entity
        var image = new Image
        {
            Id = Guid.NewGuid(),
            ListingId = defect.ListingId,
            ListingDefectId = defect.Id,
            BucketName = uploadResult.BucketName,
            ObjectKey = uploadResult.ObjectKey,
            OriginalFileName = request.Image.FileName,
            ContentType = request.Image.ContentType,
            FileSize = uploadResult.FileSize,
            AltText = $"Defect image for listing {defect.ListingId}"
        };

        await repository.CreateAsync(image, cancellationToken);

        return image.Id;
    }
}