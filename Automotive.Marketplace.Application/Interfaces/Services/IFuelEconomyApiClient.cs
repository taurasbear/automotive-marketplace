namespace Automotive.Marketplace.Application.Interfaces.Services;

public interface IFuelEconomyApiClient
{
    Task<FuelEfficiencyResult?> GetFuelEfficiencyAsync(string make, string model, int year, CancellationToken ct);
}

public record FuelEfficiencyResult(double? LitersPer100Km, double? KWhPer100Km);
