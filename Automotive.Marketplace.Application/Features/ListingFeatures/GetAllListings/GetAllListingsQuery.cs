using Automotive.Marketplace.Application.Common.Models;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;

public sealed record class GetAllListingsQuery : IRequest<PagedResult<GetAllListingsResponse>>
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;

    public Guid? MakeId { get; set; }

    public ICollection<Guid> Models { get; set; } = [];

    public Guid? MunicipalityId { get; set; }

    public bool? IsUsed { get; set; }

    public int? MinYear { get; set; }

    public int? MaxYear { get; set; }

    public int? MinPrice { get; set; }

    public int? MaxPrice { get; set; }

    public int? MinMileage { get; set; }

    public int? MaxMileage { get; set; }

    public int? MinPower { get; set; }

    public int? MaxPower { get; set; }

    public Guid? UserId { get; set; }
};
