
using AutoMapper;
using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingById;

public class GetListingByIdQueryHandler(
    IMapper mapper,
    IRepository repository,
    IImageStorageService imageStorageService) : IRequestHandler<GetListingByIdQuery, GetListingByIdResponse>
{
    public async Task<GetListingByIdResponse> Handle(GetListingByIdQuery request, CancellationToken cancellationToken)
    {
        var listing = await repository.GetByIdAsync<Listing>(request.Id, cancellationToken);

        var response = mapper.Map<GetListingByIdResponse>(listing);
        foreach (var image in listing.Images)
        {
            var imageUrl = await imageStorageService.GetPresignedUrlAsync(image.ObjectKey);
            var responseImage = new GetListingByIdResponse.Image
            {
                Url = imageUrl,
                AltText = image.AltText
            };
            response.Images.Add(responseImage);
        }
        return response;
    }
}