
using AutoMapper;
using Automotive.Marketplace.Application.Common.Exceptions;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingById;

public class GetListingByIdQueryHandler(
    IMapper mapper,
    IRepository repository,
    IImageStorageService imageStorageService) : IRequestHandler<GetListingByIdQuery, GetListingByIdResponse>
{
    public async Task<GetListingByIdResponse> Handle(GetListingByIdQuery request, CancellationToken cancellationToken)
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
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new DbEntityNotFoundException(nameof(Listing), request.Id);

        var response = mapper.Map<GetListingByIdResponse>(listing);

        var images = new List<Automotive.Marketplace.Application.Models.ImageDto>();
        foreach (var image in listing.Images)
        {
            images.Add(new Automotive.Marketplace.Application.Models.ImageDto
            {
                Url = await imageStorageService.GetPresignedUrlAsync(image.ObjectKey),
                AltText = image.AltText
            });
        }
        response.Images = images;

        return response;
    }
}