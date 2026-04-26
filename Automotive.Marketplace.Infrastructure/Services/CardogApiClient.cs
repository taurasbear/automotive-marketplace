using Automotive.Marketplace.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Automotive.Marketplace.Infrastructure.Services;

public class CardogApiClient(HttpClient httpClient, ILogger<CardogApiClient> logger) : ICardogApiClient
{
    public async Task<CardogMarketResult?> GetMarketOverviewAsync(
        string make, string model, int year, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"market/{Uri.EscapeDataString(make)}/{Uri.EscapeDataString(model)}/{year}/overview";
            var response = await httpClient.GetFromJsonAsync<MarketOverviewResponse>(url, cancellationToken);
            if (response is null) return null;
            return new CardogMarketResult(response.MedianPrice, response.TotalListings);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "CarDog market call failed for {Make} {Model} {Year}", make, model, year);
            return null;
        }
    }

    private class MarketOverviewResponse
    {
        [JsonPropertyName("medianPrice")]
        public decimal MedianPrice { get; set; }

        [JsonPropertyName("totalListings")]
        public int TotalListings { get; set; }
    }
}
