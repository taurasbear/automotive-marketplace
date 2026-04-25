namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingScore;

public class GetListingScoreResponse
{
    public int OverallScore { get; init; }
    public ScoreFactor Value { get; init; } = null!;
    public ScoreFactor Efficiency { get; init; } = null!;
    public ScoreFactor Reliability { get; init; } = null!;
    public ScoreFactor Mileage { get; init; } = null!;
    public bool HasMissingFactors { get; init; }
    public List<string> MissingFactors { get; init; } = [];
}

public record ScoreFactor(double Score, string Status, double Weight);

public record ScoreWeights(double Value, double Efficiency, double Reliability, double Mileage);
