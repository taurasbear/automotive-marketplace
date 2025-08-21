using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;

public sealed record class GetAllListingsQuery : IRequest<IEnumerable<GetAllListingsResponse>>
{
    public Guid? MakeId { get; set; }

    public Guid? ModelId { get; set; }

    public string? City { get; set; }

    public bool? IsUsed { get; set; }

    public int? YearFrom { get; set; }

    public int? YearTo { get; set; }

    public int? PriceFrom { get; set; }

    public int? PriceTo { get; set; }
};
