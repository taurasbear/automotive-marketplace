namespace Automotive.Marketplace.Application.Interfaces.Services;

public interface IOpenAiClient
{
    Task<string?> GetResponseAsync(string prompt, CancellationToken cancellationToken);
}
