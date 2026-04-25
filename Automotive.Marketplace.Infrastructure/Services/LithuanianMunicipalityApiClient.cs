using Automotive.Marketplace.Application.Interfaces.Services;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Automotive.Marketplace.Infrastructure.Services;

public class LithuanianMunicipalityApiClient(HttpClient httpClient) : IMunicipalityApiClient
{
    private const string ApiUrl =
        "https://get.data.gov.lt/datasets/gov/rc/ar/grasavivaldybe/GraSavivaldybe/:format/json";

    public async Task<IEnumerable<MunicipalityDto>> FetchMunicipalitiesAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<ApiResponse>(ApiUrl, cancellationToken);
        return response?.Data?
                   .Select(item => Guid.TryParse(item.RawId, out var id)
                       ? new MunicipalityDto(id, item.Pavadinimas)
                       : null)
                   .OfType<MunicipalityDto>()
               ?? [];
    }

    private class ApiResponse
    {
        [JsonPropertyName("_data")]
        public List<ApiItem> Data { get; set; } = [];
    }

    private class ApiItem
    {
        [JsonPropertyName("_id")]
        public string? RawId { get; set; }

        [JsonPropertyName("pavadinimas")]
        public string Pavadinimas { get; set; } = string.Empty;
    }
}
