using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;

public sealed record class GetAllListingsQuery : IRequest<IEnumerable<GetAllListingsResponse>>
{
    public Guid? MakeId { get; set; }

    public ICollection<Guid> Models { get; set; } = [];

    public string? City { get; set; }

    public bool? IsUsed { get; set; }

    public int? MinYear { get; set; }

    public int? MaxYear { get; set; }

    public int? MinPrice { get; set; }

    public int? MaxPrice { get; set; }

    public int? MinMileage { get; set; }

    public int? MaxMileage { get; set; }

    public int? MinPower { get; set; }

    public int? MaxPower { get; set; }
};
