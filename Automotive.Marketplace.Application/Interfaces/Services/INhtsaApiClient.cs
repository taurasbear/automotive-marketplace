namespace Automotive.Marketplace.Application.Interfaces.Services;

public interface INhtsaApiClient
{
    Task<NhtsaRecallsResult?> GetRecallsAsync(string make, string model, int year, CancellationToken ct);
    Task<NhtsaComplaintsResult?> GetComplaintsAsync(string make, string model, int year, CancellationToken ct);
    Task<NhtsaSafetyRatingResult?> GetSafetyRatingAsync(string make, string model, int year, CancellationToken ct);
}

public record NhtsaRecallsResult(int Count);
public record NhtsaComplaintsResult(int Total);
public record NhtsaSafetyRatingResult(int OverallRating);
