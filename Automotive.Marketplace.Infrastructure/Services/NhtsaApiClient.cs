using Automotive.Marketplace.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Automotive.Marketplace.Infrastructure.Services;

public class NhtsaApiClient(HttpClient httpClient, ILogger<NhtsaApiClient> logger) : INhtsaApiClient
{
    public async Task<NhtsaRecallsResult?> GetRecallsAsync(
        string make, string model, int year, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"recalls/recallsByVehicle?make={Uri.EscapeDataString(make)}&model={Uri.EscapeDataString(model)}&modelYear={year}";
            var response = await httpClient.GetFromJsonAsync<RecallsResponse>(url, cancellationToken);
            return response is null ? null : new NhtsaRecallsResult(response.Count);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "NHTSA recalls call failed for {Make} {Model} {Year}", make, model, year);
            return null;
        }
    }

    public async Task<NhtsaComplaintsResult?> GetComplaintsAsync(
        string make, string model, int year, CancellationToken cancellationToken)
    {
        try
        {
            var url = $"complaints/complaintsByVehicle?make={Uri.EscapeDataString(make)}&model={Uri.EscapeDataString(model)}&modelYear={year}";
            var response = await httpClient.GetFromJsonAsync<ComplaintsResponse>(url, cancellationToken);
            return response is null ? null : new NhtsaComplaintsResult(response.Count);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "NHTSA complaints call failed for {Make} {Model} {Year}", make, model, year);
            return null;
        }
    }

    public async Task<NhtsaSafetyRatingResult?> GetSafetyRatingAsync(
        string make, string model, int year, CancellationToken cancellationToken)
    {
        try
        {
            var variantsUrl = $"SafetyRatings/modelyear/{year}/make/{Uri.EscapeDataString(make)}/model/{Uri.EscapeDataString(model)}";
            var variantsResponse = await httpClient.GetFromJsonAsync<VariantsResponse>(variantsUrl, cancellationToken);

            if (variantsResponse?.Results is not { Count: > 0 })
                return null;

            var vehicleId = variantsResponse.Results[0].VehicleId;
            var ratingUrl = $"SafetyRatings/VehicleId/{vehicleId}";
            var ratingResponse = await httpClient.GetFromJsonAsync<RatingsResponse>(ratingUrl, cancellationToken);

            if (ratingResponse?.Results is not { Count: > 0 })
                return null;

            var raw = ratingResponse.Results[0].OverallRating;
            if (!int.TryParse(raw, out var rating))
                return null; // "Not Rated" or empty

            return new NhtsaSafetyRatingResult(rating);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "NHTSA safety rating call failed for {Make} {Model} {Year}", make, model, year);
            return null;
        }
    }

    // Recalls response: Count uses uppercase C
    private class RecallsResponse
    {
        [JsonPropertyName("Count")]
        public int Count { get; set; }
    }

    // Complaints response: count uses lowercase c
    private class ComplaintsResponse
    {
        [JsonPropertyName("count")]
        public int Count { get; set; }
    }

    private class VariantsResponse
    {
        [JsonPropertyName("Results")]
        public List<VariantItem> Results { get; set; } = [];
    }

    private class VariantItem
    {
        [JsonPropertyName("VehicleId")]
        public int VehicleId { get; set; }
    }

    private class RatingsResponse
    {
        [JsonPropertyName("Results")]
        public List<RatingItem> Results { get; set; } = [];
    }

    private class RatingItem
    {
        [JsonPropertyName("OverallRating")]
        public string OverallRating { get; set; } = string.Empty;
    }
}
