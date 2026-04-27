using Automotive.Marketplace.Application.Interfaces.Data;
using Automotive.Marketplace.Application.Interfaces.Services;
using Automotive.Marketplace.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.SearchListings;

public class SearchListingsQueryHandler(
    IRepository repository,
    IImageStorageService imageStorageService)
    : IRequestHandler<SearchListingsQuery, IEnumerable<SearchListingsResponse>>
{
    public async Task<IEnumerable<SearchListingsResponse>> Handle(
        SearchListingsQuery request, CancellationToken cancellationToken)
    {
        var q = request.Q.Trim().ToLower();
        int yearValue = 0;
        var isYear = q.Length == 4 && int.TryParse(q, out yearValue);

        var listings = await repository
            .AsQueryable<Listing>()
            .Include(l => l.Variant)
                .ThenInclude(v => v.Model)
                    .ThenInclude(m => m.Make)
            .Include(l => l.Seller)
            .Include(l => l.Images)
            .Include(l => l.Municipality)
            .Where(l =>
                l.Variant.Model.Make.Name.ToLower().Contains(q) ||
                l.Variant.Model.Name.ToLower().Contains(q) ||
                (l.Variant.Model.Make.Name.ToLower() + " " + l.Variant.Model.Name.ToLower()).Contains(q) ||
                (isYear && l.Year == yearValue) ||
                l.Seller.Username.ToLower().Contains(q) ||
                l.Id.ToString() == request.Q.Trim())
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        var result = new List<SearchListingsResponse>();
        foreach (var listing in listings)
        {
            var response = new SearchListingsResponse
            {
                Id = listing.Id,
                MakeName = listing.Variant?.Model?.Make?.Name ?? string.Empty,
                ModelName = listing.Variant?.Model?.Name ?? string.Empty,
                Year = listing.Year,
                Price = listing.Price,
                Mileage = listing.Mileage,
                MunicipalityName = listing.Municipality?.Name ?? string.Empty,
                SellerName = listing.Seller?.Username ?? string.Empty,
            };

            var firstImage = listing.Images.FirstOrDefault();
            if (firstImage != null)
            {
                response = response with
                {
                    FirstImageUrl = await imageStorageService.GetPresignedUrlAsync(firstImage.ObjectKey)
                };
            }

            result.Add(response);
        }

        return result;
    }
}
