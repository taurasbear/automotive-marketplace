namespace Automotive.Marketplace.Application.Interfaces.Services;

public interface INhtsaApiClient
{
    Task<NhtsaRecallsResult?> GetRecallsAsync(string make, string model, int year, CancellationToken cancellationToken);
    Task<NhtsaComplaintsResult?> GetComplaintsAsync(string make, string model, int year, CancellationToken cancellationToken);
    Task<NhtsaSafetyRatingResult?> GetSafetyRatingAsync(string make, string model, int year, CancellationToken cancellationToken);
}

public record NhtsaRecallsResult(int RecallCount);
public record NhtsaComplaintsResult(int ComplaintCount);
public record NhtsaSafetyRatingResult(int OverallRating);
