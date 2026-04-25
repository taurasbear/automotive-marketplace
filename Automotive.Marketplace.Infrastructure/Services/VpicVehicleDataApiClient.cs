using Automotive.Marketplace.Application.Interfaces.Services;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Automotive.Marketplace.Infrastructure.Services;

public class VpicVehicleDataApiClient(HttpClient httpClient) : IVehicleDataApiClient
{
    private const string MakesUrl =
        "https://vpic.nhtsa.dot.gov/api/vehicles/GetMakesForVehicleType/car?format=json";

    private static string ModelsUrl(int makeId) =>
        $"https://vpic.nhtsa.dot.gov/api/vehicles/GetModelsForMakeIdYear/makeId/{makeId}/vehicletype/car?format=json";

    public async Task<IEnumerable<VpicMakeDto>> FetchCarMakesAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<MakesResponse>(MakesUrl, cancellationToken);
        return response?.Results?
                   .Select(r => new VpicMakeDto(r.MakeId, r.MakeName))
               ?? [];
    }

    public async Task<IEnumerable<VpicModelDto>> FetchModelsForMakeAsync(
        int vpicMakeId,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<ModelsResponse>(
            ModelsUrl(vpicMakeId), cancellationToken);
        return response?.Results?
                   .Select(r => new VpicModelDto(r.ModelId, r.ModelName, r.MakeId))
               ?? [];
    }

    private class MakesResponse
    {
        [JsonPropertyName("Results")]
        public List<MakeResult> Results { get; set; } = [];
    }

    private class MakeResult
    {
        [JsonPropertyName("MakeId")]
        public int MakeId { get; set; }

        [JsonPropertyName("MakeName")]
        public string MakeName { get; set; } = string.Empty;
    }

    private class ModelsResponse
    {
        [JsonPropertyName("Results")]
        public List<ModelResult> Results { get; set; } = [];
    }

    private class ModelResult
    {
        [JsonPropertyName("Model_ID")]
        public int ModelId { get; set; }

        [JsonPropertyName("Model_Name")]
        public string ModelName { get; set; } = string.Empty;

        [JsonPropertyName("Make_ID")]
        public int MakeId { get; set; }
    }
}
