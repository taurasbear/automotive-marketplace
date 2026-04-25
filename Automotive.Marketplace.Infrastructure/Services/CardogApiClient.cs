using Automotive.Marketplace.Application.Interfaces.Services;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Automotive.Marketplace.Infrastructure.Services;

public class CardogApiClient(HttpClient httpClient) : ICardogApiClient
{
    public async Task<CardogEfficiencyResult?> GetEfficiencyAsync(
        string make, string model, int year, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"efficiency/{Uri.EscapeDataString(make)}/{Uri.EscapeDataString(model)}/{year}";
            var response = await httpClient.GetFromJsonAsync<EfficiencyResponse>(url, cancellationToken);
            if (response?.Data is not { Count: > 0 }) return null;

            var avgLiters = response.Data
                .Where(d => d.CombinedLPer100km.HasValue)
                .Select(d => d.CombinedLPer100km!.Value)
                .DefaultIfEmpty()
                .Average();

            var avgKwh = response.Data
                .Where(d => d.CombinedKwhPer100km.HasValue)
                .Select(d => d.CombinedKwhPer100km!.Value)
                .DefaultIfEmpty()
                .Average();

            return new CardogEfficiencyResult(
                avgLiters > 0 ? avgLiters : null,
                avgKwh > 0 ? avgKwh : null);
        }
        catch
        {
            return null;
        }
    }

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
        catch
        {
            return null;
        }
    }

    public async Task<CardogReliabilityResult?> GetReliabilityAsync(
        string make, string model, int year, CancellationToken cancellationToken)
    {
        try
        {
            var makeEncoded = Uri.EscapeDataString(make.ToUpperInvariant());
            var modelEncoded = Uri.EscapeDataString(model.ToUpperInvariant());

            var recallsResponse = await httpClient.GetFromJsonAsync<CountResponse>(
                $"recalls/us/search?makes={makeEncoded}&models={modelEncoded}&year={year}&limit=1",
                cancellationToken);

            var crashesResponse = await httpClient.GetFromJsonAsync<CountResponse>(
                $"complaints/search?makes={makeEncoded}&models={modelEncoded}&yearMin={year}&yearMax={year}&crashInvolved=true&limit=1",
                cancellationToken);

            var injuriesResponse = await httpClient.GetFromJsonAsync<CountResponse>(
                $"complaints/search?makes={makeEncoded}&models={modelEncoded}&yearMin={year}&yearMax={year}&hasInjuries=true&limit=1",
                cancellationToken);

            return new CardogReliabilityResult(
                recallsResponse?.Count ?? 0,
                crashesResponse?.Count ?? 0,
                injuriesResponse?.Count ?? 0);
        }
        catch
        {
            return null;
        }
    }

    // Private JSON models
    private class EfficiencyResponse
    {
        [JsonPropertyName("data")]
        public List<EfficiencyRecord> Data { get; set; } = [];
    }

    private class EfficiencyRecord
    {
        [JsonPropertyName("combinedLPer100km")]
        public double? CombinedLPer100km { get; set; }

        [JsonPropertyName("combinedKwhPer100km")]
        public double? CombinedKwhPer100km { get; set; }
    }

    private class MarketOverviewResponse
    {
        [JsonPropertyName("medianPrice")]
        public decimal MedianPrice { get; set; }

        [JsonPropertyName("totalListings")]
        public int TotalListings { get; set; }
    }

    private class CountResponse
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
