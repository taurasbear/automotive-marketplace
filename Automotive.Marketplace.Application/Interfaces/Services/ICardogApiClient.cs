namespace Automotive.Marketplace.Application.Interfaces.Services;

public interface ICardogApiClient
{
    Task<CardogMarketResult?> GetMarketOverviewAsync(string make, string model, int year, CancellationToken cancellationToken);
}

public record CardogMarketResult(
    decimal MedianPrice,
    int TotalListings);
