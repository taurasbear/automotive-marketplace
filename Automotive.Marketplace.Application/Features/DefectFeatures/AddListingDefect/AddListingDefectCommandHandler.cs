using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.DefectFeatures.AddListingDefect;

public class AddListingDefectCommandHandler(IRepository repository)
    : IRequestHandler<AddListingDefectCommand, Guid>
{
    public async Task<Guid> Handle(AddListingDefectCommand request, CancellationToken cancellationToken)
    {
        // Validate listing exists
        var listing = await repository.GetByIdAsync<Listing>(request.ListingId, cancellationToken);
        if (listing == null)
        {
            throw new DbEntityNotFoundException(nameof(Listing), request.ListingId);
        }

        // Validate DefectCategory exists if provided
        if (request.DefectCategoryId.HasValue)
        {
            var defectCategory = await repository.GetByIdAsync<DefectCategory>(request.DefectCategoryId.Value, cancellationToken);
            if (defectCategory == null)
            {
                throw new DbEntityNotFoundException(nameof(DefectCategory), request.DefectCategoryId.Value);
            }
        }

        // Create ListingDefect entity
        var listingDefect = new ListingDefect
        {
            Id = Guid.NewGuid(),
            ListingId = request.ListingId,
            DefectCategoryId = request.DefectCategoryId,
            CustomName = request.CustomName
        };

        await repository.CreateAsync(listingDefect, cancellationToken);

        return listingDefect.Id;
    }
}