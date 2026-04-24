using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.DefectFeatures.RemoveDefectImage;

public class RemoveDefectImageCommandHandler(IRepository repository, IImageStorageService imageStorageService)
    : IRequestHandler<RemoveDefectImageCommand>
{
    public async Task Handle(RemoveDefectImageCommand request, CancellationToken cancellationToken)
    {
        // Get image by Id
        var image = await repository.GetByIdAsync<Image>(request.Id, cancellationToken);
        if (image == null)
        {
            throw new DbEntityNotFoundException(nameof(Image), request.Id);
        }

        // Delete from storage
        await imageStorageService.DeleteImageAsync(image.ObjectKey);

        // Delete from database
        await repository.DeleteAsync(image, cancellationToken);
    }
}