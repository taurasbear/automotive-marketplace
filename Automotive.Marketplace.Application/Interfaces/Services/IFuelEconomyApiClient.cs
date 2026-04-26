namespace Automotive.Marketplace.Application.Interfaces.Services;

public interface IFuelEconomyApiClient
{
    Task<FuelEconomyEfficiencyResult?> GetFuelEfficiencyAsync(string make, string model, int year, CancellationToken cancellationToken);
}

public record FuelEconomyEfficiencyResult(double? LitersPer100Km, double? KWhPer100Km);
