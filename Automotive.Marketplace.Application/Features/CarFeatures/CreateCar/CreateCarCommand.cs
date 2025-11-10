using Automotive.Marketplace.Domain.Enums;
using MediatR;

namespace Automotive.Marketplace.Application.Features.CarFeatures.CreateCar;

public sealed record CreateCarCommand : IRequest
{
    public required int Year { get; set; }

    public required Fuel Fuel { get; set; }

    public required Transmission Transmission { get; set; }

    public required BodyType BodyType { get; set; }

    public required Drivetrain Drivetrain { get; set; }

    public required int DoorCount { get; set; }

    public required Guid ModelId { get; set; }
};