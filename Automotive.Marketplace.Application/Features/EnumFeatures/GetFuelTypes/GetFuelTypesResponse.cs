namespace Automotive.Marketplace.Application.Features.EnumFeatures.GetFuelTypes;

public sealed record GetFuelTypesResponse
{
    public string FuelType { get; set; } = string.Empty;
}
