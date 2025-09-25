namespace Automotive.Marketplace.Application.Features.CarFeatures.GetCarById;

public sealed record GetCarByIdResponse
{
    public string Year { get; set; } = string.Empty;

    public string FuelType { get; set; } = string.Empty;

    public string Transmission { get; set; } = string.Empty;

    public string BodyType { get; set; } = string.Empty;

    public string Drivetrain { get; set; } = string.Empty;

    public int DoorCount { get; set; }

    public Guid MakeId { get; set; }

    public Guid ModelId { get; set; }
}