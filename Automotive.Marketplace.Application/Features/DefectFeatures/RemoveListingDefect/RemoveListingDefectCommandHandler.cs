using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.DefectFeatures.RemoveListingDefect;

public class RemoveListingDefectCommandHandler(IRepository repository, IImageStorageService imageStorageService)
    : IRequestHandler<RemoveListingDefectCommand>
{
    public async Task Handle(RemoveListingDefectCommand request, CancellationToken cancellationToken)
    {
        // Load defect with images
        var defect = await repository
            .AsQueryable<ListingDefect>()
            .Include(ld => ld.Images)
            .FirstOrDefaultAsync(ld => ld.Id == request.Id, cancellationToken);

        if (defect == null)
        {
            throw new DbEntityNotFoundException(nameof(ListingDefect), request.Id);
        }

        // Delete all images from storage and database
        foreach (var image in defect.Images)
        {
            await imageStorageService.DeleteImageAsync(image.ObjectKey);
            await repository.DeleteAsync(image, cancellationToken);
        }

        // Delete the defect itself
        await repository.DeleteAsync(defect, cancellationToken);
    }
}