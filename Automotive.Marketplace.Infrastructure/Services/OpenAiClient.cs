using Automotive.Marketplace.Application.Interfaces.Services;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Automotive.Marketplace.Infrastructure.Services;

public class OpenAiClient(HttpClient httpClient) : IOpenAiClient
{
    private const string Model = "gpt-5.4-mini";

    public async Task<string?> GetResponseAsync(string prompt, CancellationToken cancellationToken)
    {
        try
        {
            var request = new { model = Model, input = prompt };
            var response = await httpClient.PostAsJsonAsync("responses", request, cancellationToken);
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<OpenAiResponse>(cancellationToken);
            return result?.Output?.FirstOrDefault()?.Content?.FirstOrDefault(c => c.Type == "output_text")?.Text;
        }
        catch
        {
            return null;
        }
    }

    private class OpenAiResponse
    {
        [JsonPropertyName("output")]
        public List<OutputItem> Output { get; set; } = [];
    }

    private class OutputItem
    {
        [JsonPropertyName("content")]
        public List<ContentItem> Content { get; set; } = [];
    }

    private class ContentItem
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }
}
