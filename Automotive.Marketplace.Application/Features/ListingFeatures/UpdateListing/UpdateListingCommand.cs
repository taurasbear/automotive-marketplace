using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.UpdateListing;

public sealed record UpdateListingCommand : IRequest
{
    public required Guid Id { get; set; }

    public required decimal Price { get; set; }

    public string? Description { get; set; }

    public string? Colour { get; set; }

    public string? Vin { get; set; }

    public required int Power { get; set; }

    public int EngineSize { get; set; }

    public required int Mileage { get; set; }

    public required bool IsSteeringWheelRight { get; set; }

    public required string City { get; set; } = string.Empty;

    public required bool IsUsed { get; set; }

    public required int Year { get; set; }
};