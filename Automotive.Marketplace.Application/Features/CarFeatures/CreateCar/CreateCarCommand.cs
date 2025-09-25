using Automotive.Marketplace.Domain.Enums;
using MediatR;

namespace Automotive.Marketplace.Application.Features.CarFeatures.CreateCar;

public sealed record CreateCarCommand : IRequest
{
    public DateTime Year { get; set; }

    public Fuel Fuel { get; set; }

    public Transmission Transmission { get; set; }

    public BodyType BodyType { get; set; }

    public Drivetrain Drivetrain { get; set; }

    public int DoorCount { get; set; }

    public Guid ModelId { get; set; }
};