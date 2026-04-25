
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
            .Include(l => l.Defects)
                .ThenInclude(d => d.DefectCategory)
            .Include(l => l.Defects)
                .ThenInclude(d => d.Images)
            .Include(l => l.Municipality)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken)
            ?? throw new DbEntityNotFoundException(nameof(Listing), request.Id);

        var response = mapper.Map<GetListingByIdResponse>(listing);

        var images = new List<Automotive.Marketplace.Application.Models.ImageDto>();
        foreach (var image in listing.Images.Where(i => i.ListingDefectId == null))
        {
            images.Add(new Automotive.Marketplace.Application.Models.ImageDto
            {
                Url = await imageStorageService.GetPresignedUrlAsync(image.ObjectKey),
                AltText = image.AltText
            });
        }
        response.Images = images;

        var defects = new List<Automotive.Marketplace.Application.Models.ListingDefectDto>();
        foreach (var defect in listing.Defects)
        {
            var defectImages = new List<Automotive.Marketplace.Application.Models.ImageDto>();
            foreach (var img in defect.Images)
            {
                defectImages.Add(new Automotive.Marketplace.Application.Models.ImageDto
                {
                    Url = await imageStorageService.GetPresignedUrlAsync(img.ObjectKey),
                    AltText = img.AltText,
                });
            }
            defects.Add(new Automotive.Marketplace.Application.Models.ListingDefectDto
            {
                Id = defect.Id,
                DefectCategoryId = defect.DefectCategoryId,
                DefectCategoryName = defect.DefectCategory?.Name,
                CustomName = defect.CustomName,
                Images = defectImages,
            });
        }
        response.Defects = defects;

        return response;
    }
}