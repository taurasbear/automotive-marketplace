using Automotive.Marketplace.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Automotive.Marketplace.Infrastructure.Services;

public class FuelEconomyApiClient(HttpClient httpClient, ILogger<FuelEconomyApiClient> logger) : IFuelEconomyApiClient
{
    // MPG (US) to L/100km: 235.215 / mpg
    private const double MpgToL100KmFactor = 235.215;
    // kWh/100mi to kWh/100km: multiply by 0.621371
    private const double KwhPer100MiToKm = 0.621371;

    public async Task<FuelEconomyEfficiencyResult?> GetFuelEfficiencyAsync(
        string make, string model, int year, CancellationToken cancellationToken)
    {
        try
        {
            // Step 1: Get model list for this make/year
            var modelsUrl = $"vehicle/menu/model?year={year}&make={Uri.EscapeDataString(make)}";
            var modelsJson = await httpClient.GetStringAsync(modelsUrl, cancellationToken);
            var matchedModel = FindBestModelMatch(modelsJson, model);
            if (matchedModel is null) return null;

            // Step 2: Get vehicle options (IDs) for the matched model
            var optionsUrl = $"vehicle/menu/options?year={year}&make={Uri.EscapeDataString(make)}&model={Uri.EscapeDataString(matchedModel)}";
            var optionsJson = await httpClient.GetStringAsync(optionsUrl, cancellationToken);
            var vehicleId = ExtractFirstVehicleId(optionsJson);
            if (vehicleId is null) return null;

            // Step 3: Get the vehicle record with MPG data
            var vehicleUrl = $"vehicle/{vehicleId}";
            var vehicleJson = await httpClient.GetStringAsync(vehicleUrl, cancellationToken);
            return ParseEfficiencyResult(vehicleJson);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "FuelEconomy.gov call failed for {Make} {Model} {Year}", make, model, year);
            return null;
        }
    }

    private static string? FindBestModelMatch(string json, string targetModel)
    {
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("menuItem", out var menuItemElement))
            return null;

        var items = ParseMenuItems(menuItemElement);
        var prefix = targetModel.Trim();

        return items
            .Select(i => i.Value)
            .FirstOrDefault(v => v.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    private static string? ExtractFirstVehicleId(string json)
    {
        if (json is "null" or "") return null;
        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("menuItem", out var menuItemElement))
            return null;

        var items = ParseMenuItems(menuItemElement);
        return items.FirstOrDefault()?.Value;
    }

    private static FuelEconomyEfficiencyResult? ParseEfficiencyResult(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        // Check for EV (combE = kWh/100 miles, comb08 = MPGe)
        if (root.TryGetProperty("combE", out var combEEl) &&
            double.TryParse(combEEl.GetString(), out var combEMi) && combEMi > 0)
        {
            var kwhPer100Km = combEMi * KwhPer100MiToKm;
            return new FuelEconomyEfficiencyResult(LitersPer100Km: null, KWhPer100Km: kwhPer100Km);
        }

        // ICE / hybrid — use combined MPG
        if (root.TryGetProperty("comb08", out var comb08El) &&
            double.TryParse(comb08El.GetString(), out var mpg) && mpg > 0)
        {
            var lPer100Km = MpgToL100KmFactor / mpg;
            return new FuelEconomyEfficiencyResult(LitersPer100Km: lPer100Km, KWhPer100Km: null);
        }

        return null;
    }

    // fueleconomy.gov returns a single object (not array) when only one item exists.
    private static List<MenuItem> ParseMenuItems(JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Array)
        {
            return element.Deserialize<List<MenuItem>>() ?? [];
        }
        if (element.ValueKind == JsonValueKind.Object)
        {
            var single = element.Deserialize<MenuItem>();
            return single is null ? [] : [single];
        }
        return [];
    }

    private class MenuItem
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
}
