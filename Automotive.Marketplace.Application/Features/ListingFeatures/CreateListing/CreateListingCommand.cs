using Automotive.Marketplace.Domain.Enums;
using MediatR;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.CreateListing;

public sealed record class CreateListingCommand : IRequest
{
    public decimal Price { get; set; }

    public string? Description { get; set; }

    public string? Colour { get; set; }

    public string? Vin { get; set; }

    public int Power { get; set; }

    public int EngineSize { get; set; }

    public int Mileage { get; set; }

    public bool IsSteeringWheelRight { get; set; }

    public Guid ModelId { get; set; }

    public string City { get; set; } = string.Empty;

    public bool IsUsed { get; set; }

    public int Year { get; set; }

    public Transmission Transmission { get; set; }

    public Fuel Fuel { get; set; }

    public int DoorCount { get; set; }

    public Guid UserId { get; set; }

    public BodyType BodyType { get; set; }

    public Drivetrain Drivetrain { get; set; }
}