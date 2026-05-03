using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.UpdateListing;

public sealed record UpdateListingCommand : IRequest
{
    public Guid Id { get; set; }

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public string? Colour { get; set; }

    public string? Vin { get; set; }

    public int PowerKw { get; set; }

    public int EngineSizeMl { get; set; }

    public int Mileage { get; set; }

    public bool IsSteeringWheelRight { get; set; }

    public Guid MunicipalityId { get; set; }

    public bool IsUsed { get; set; }

    public int Year { get; set; }

    public Guid CurrentUserId { get; set; }

    public IReadOnlyList<string> Permissions { get; set; } = [];
};