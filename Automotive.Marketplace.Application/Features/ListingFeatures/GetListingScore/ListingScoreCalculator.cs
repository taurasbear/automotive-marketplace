using Automotive.Marketplace.Application.Interfaces.Services;

namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingScore;

public static class ListingScoreCalculator
{
    private const double ValueWeight = 0.30;
    private const double EfficiencyWeight = 0.25;
    private const double ReliabilityWeight = 0.25;
    private const double MileageWeight = 0.20;

    public static GetListingScoreResponse Calculate(
        decimal listingPrice,
        int year,
        int mileageKm,
        CardogMarketResult? market,
        CardogEfficiencyResult? efficiency,
        CardogReliabilityResult? reliability,
        ScoreWeights? weights = null)
    {
        var w = weights ?? new ScoreWeights(ValueWeight, EfficiencyWeight, ReliabilityWeight, MileageWeight);

        var valueFactor = market != null
            ? ScoreValue(listingPrice, market.MedianPrice, w.Value)
            : MissingFactor(w.Value);

        var efficiencyFactor = efficiency != null
            ? ScoreEfficiency(efficiency, w.Efficiency)
            : MissingFactor(w.Efficiency);

        var reliabilityFactor = reliability != null
            ? ScoreReliability(reliability, w.Reliability)
            : MissingFactor(w.Reliability);

        var mileageFactor = ScoreMileage(year, mileageKm, w.Mileage);

        var allFactors = new[] { valueFactor, efficiencyFactor, reliabilityFactor, mileageFactor };
        var scoredFactors = allFactors.Where(f => f.Status == "scored").ToArray();
        var totalWeight = scoredFactors.Sum(f => f.Weight);
        var overallScore = totalWeight > 0
            ? (int)Math.Round(scoredFactors.Sum(f => f.Score * f.Weight) / totalWeight)
            : 50;

        var missingFactors = new List<string>();
        if (valueFactor.Status == "missing") missingFactors.Add("Market Value");
        if (efficiencyFactor.Status == "missing") missingFactors.Add("Efficiency");
        if (reliabilityFactor.Status == "missing") missingFactors.Add("Reliability");

        return new GetListingScoreResponse
        {
            OverallScore = overallScore,
            Value = valueFactor,
            Efficiency = efficiencyFactor,
            Reliability = reliabilityFactor,
            Mileage = mileageFactor,
            HasMissingFactors = missingFactors.Count > 0,
            MissingFactors = missingFactors,
        };
    }

    private static ScoreFactor ScoreValue(decimal listingPrice, decimal medianPrice, double weight)
    {
        var ratio = (double)((medianPrice - listingPrice) / medianPrice);
        var clamped = Math.Clamp(ratio, -0.5, 0.3);
        var score = (clamped + 0.5) / 0.8 * 100.0;
        return new ScoreFactor(Score: score, Status: "scored", Weight: weight);
    }

    private static ScoreFactor ScoreEfficiency(CardogEfficiencyResult efficiency, double weight)
    {
        double score;
        if (efficiency.KWhPer100Km.HasValue)
        {
            score = Math.Clamp((25.0 - efficiency.KWhPer100Km.Value) / 10.0, 0, 1) * 100.0;
        }
        else if (efficiency.LitersPer100Km.HasValue)
        {
            score = Math.Clamp((12.0 - efficiency.LitersPer100Km.Value) / 6.0, 0, 1) * 100.0;
        }
        else
        {
            return MissingFactor(weight);
        }
        return new ScoreFactor(Score: score, Status: "scored", Weight: weight);
    }

    private static ScoreFactor ScoreReliability(CardogReliabilityResult reliability, double weight)
    {
        var penalty = reliability.RecallCount * 2 + reliability.ComplaintCrashes * 3 + reliability.ComplaintInjuries * 2;
        var score = 100.0 - Math.Clamp(penalty / 50.0, 0, 1) * 100.0;
        return new ScoreFactor(Score: score, Status: "scored", Weight: weight);
    }

    private static ScoreFactor ScoreMileage(int year, int mileageKm, double weight)
    {
        var ageYears = Math.Max(DateTime.UtcNow.Year - year, 0);
        if (ageYears == 0)
            return new ScoreFactor(Score: 100.0, Status: "scored", Weight: weight);

        var expectedKm = ageYears * 15000.0;
        var score = Math.Clamp((expectedKm - mileageKm) / expectedKm, 0, 1) * 100.0;
        return new ScoreFactor(Score: score, Status: "scored", Weight: weight);
    }

    private static ScoreFactor MissingFactor(double weight) =>
        new(Score: 50, Status: "missing", Weight: weight);
}
