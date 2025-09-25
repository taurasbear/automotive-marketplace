using Automotive.Marketplace.Domain.Enums;
using MediatR;

namespace Automotive.Marketplace.Application.Features.CarFeatures.UpdateCar;

public sealed record UpdateCarCommand : IRequest
{
    public Guid Id { get; set; }

    public DateTime Year { get; set; }

    public Fuel Fuel { get; set; }

    public Transmission Transmission { get; set; }

    public BodyType BodyType { get; set; }

    public Drivetrain Drivetrain { get; set; }

    public int DoorCount { get; set; }

    public Guid ModelId { get; set; }
};