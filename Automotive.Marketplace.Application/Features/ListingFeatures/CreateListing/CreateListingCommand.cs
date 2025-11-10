using Automotive.Marketplace.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.CreateListing;

public sealed record class CreateListingCommand : IRequest
{
    public required Guid ModelId { get; set; }

    public required decimal Price { get; set; }

    public string? Description { get; set; }

    public string? Colour { get; set; }

    public string? Vin { get; set; }

    public required int Power { get; set; }

    public required int EngineSize { get; set; }

    public required int Mileage { get; set; }

    public required bool IsSteeringWheelRight { get; set; }

    public required string City { get; set; } = string.Empty;

    public required bool IsUsed { get; set; }

    public required int Year { get; set; }

    public required Transmission Transmission { get; set; }

    public required Fuel Fuel { get; set; }

    public required int DoorCount { get; set; }

    public required BodyType BodyType { get; set; }

    public required Drivetrain Drivetrain { get; set; }

    public required Guid UserId { get; set; }

    public List<IFormFile> Images { get; set; } = [];
}