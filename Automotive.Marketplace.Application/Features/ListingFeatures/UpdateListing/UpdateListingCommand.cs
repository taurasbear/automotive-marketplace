using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.UpdateListing;

public sealed record UpdateListingCommand : IRequest
{
    public Guid Id { get; set; }

    public Guid ModelId { get; set; }

    public decimal Price { get; set; }

    public string? Description { get; set; }

    public string? Colour { get; set; }

    public string? Vin { get; set; }

    public int Power { get; set; }

    public int EngineSize { get; set; }

    public int Mileage { get; set; }

    public bool IsSteeringWheelRight { get; set; }

    public string City { get; set; } = string.Empty;

    public bool IsUsed { get; set; }

    public int Year { get; set; }
};