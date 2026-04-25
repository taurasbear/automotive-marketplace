namespace Automotive.Marketplace.Application.Interfaces.Services;

public interface ICardogApiClient
{
    Task<CardogEfficiencyResult?> GetEfficiencyAsync(string make, string model, int year, CancellationToken cancellationToken);
    Task<CardogMarketResult?> GetMarketOverviewAsync(string make, string model, int year, CancellationToken cancellationToken);
    Task<CardogReliabilityResult?> GetReliabilityAsync(string make, string model, int year, CancellationToken cancellationToken);
}

public record CardogEfficiencyResult(
    double? LitersPer100Km,
    double? KWhPer100Km);

public record CardogMarketResult(
    decimal MedianPrice,
    int TotalListings);

public record CardogReliabilityResult(
    int RecallCount,
    int ComplaintCrashes,
    int ComplaintInjuries);
