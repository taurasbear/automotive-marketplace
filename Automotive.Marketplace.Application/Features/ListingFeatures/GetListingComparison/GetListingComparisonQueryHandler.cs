using AutoMapper;
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingById;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Application.Models;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingComparison;

public class GetListingComparisonQueryHandler(
    IMapper mapper,
    IRepository repository,
    IImageStorageService imageStorageService)
    : IRequestHandler<GetListingComparisonQuery, GetListingComparisonResponse>
{
    public async Task<GetListingComparisonResponse> Handle(
        GetListingComparisonQuery request, CancellationToken cancellationToken)
    {
        var listingA = await FetchListingAsync(request.ListingAId, cancellationToken);
        var listingB = await FetchListingAsync(request.ListingBId, cancellationToken);

        return new GetListingComparisonResponse { ListingA = listingA, ListingB = listingB };
    }

    private async Task<GetListingByIdResponse> FetchListingAsync(Guid id, CancellationToken cancellationToken)
    {
        var listing = await repository
            .AsQueryable<Listing>()
            .Include(l => l.Variant)
                .ThenInclude(v => v.Model)
                    .ThenInclude(m => m.Make)
            .Include(l => l.Variant)
                .ThenInclude(v => v.Fuel)
            .Include(l => l.Variant)
                .ThenInclude(v => v.Transmission)
            .Include(l => l.Variant)
                .ThenInclude(v => v.BodyType)
            .Include(l => l.Drivetrain)
            .Include(l => l.Seller)
            .Include(l => l.Images)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken)
            ?? throw new DbEntityNotFoundException(nameof(Listing), id);

        var response = mapper.Map<GetListingByIdResponse>(listing);

        var images = new List<ImageDto>();
        foreach (var image in listing.Images)
        {
            images.Add(new ImageDto
            {
                Url = await imageStorageService.GetPresignedUrlAsync(image.ObjectKey),
                AltText = image.AltText
            });
        }
        response.Images = images;

        return response;
    }
}
