namespace Automotive.Marketplace.Application.Features.CarFeatures.GetAllCars;

public sealed record GetAllCarsResponse
{
    public Guid Id { get; set; }

    public string Year { get; set; } = string.Empty;

    public string FuelType { get; set; } = string.Empty;

    public string Transmission { get; set; } = string.Empty;
}