using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingScore;
using Automotive.Marketplace.Application.Interfaces.Services;
using FluentAssertions;

namespace Automotive.Marketplace.Tests.Features.UnitTests;

public class ListingScoreCalculatorTests
{
    // ── Value factor ──────────────────────────────────────────────────────────

    [Fact]
    public void Calculate_ListingBelowMarket_ReturnsHighValueScore()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 8000m, year: 2020, mileageKm: 60000, defectCount: 0,
            market: new CardogMarketResult(MedianPrice: 10000m, TotalListings: 50),
            efficiency: null, recalls: null, complaints: null, safetyRating: null);

        // (10000 - 8000) / 10000 = 0.20 → (0.20 + 0.5) / 0.8 * 100 = 87.5
        result.Value.Score.Should().BeApproximately(87.5, 0.5);
        result.Value.Status.Should().Be("scored");
    }

    [Fact]
    public void Calculate_ListingAtMarket_ReturnsApprox62Score()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m, year: 2020, mileageKm: 60000, defectCount: 0,
            market: new CardogMarketResult(MedianPrice: 10000m, TotalListings: 50),
            efficiency: null, recalls: null, complaints: null, safetyRating: null);

        // (0 + 0.5) / 0.8 * 100 = 62.5
        result.Value.Score.Should().BeApproximately(62.5, 0.5);
    }

    [Fact]
    public void Calculate_NullMarket_ValueFactorIsMissing()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 8000m, year: 2020, mileageKm: 60000, defectCount: 0,
            market: null, efficiency: null, recalls: null, complaints: null, safetyRating: null);

        result.Value.Status.Should().Be("missing");
        result.Value.Score.Should().Be(50);
        result.HasMissingFactors.Should().BeTrue();
        result.MissingFactors.Should().Contain("Market Value");
    }

    // ── Efficiency factor ─────────────────────────────────────────────────────

    [Fact]
    public void Calculate_EfficientIceVehicle_ReturnsHighEfficiencyScore()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m, year: 2020, mileageKm: 60000, defectCount: 0,
            market: null,
            efficiency: new FuelEconomyEfficiencyResult(LitersPer100Km: 6.0, KWhPer100Km: null),
            recalls: null, complaints: null, safetyRating: null);

        // (12 - 6) / 6 * 100 = 100
        result.Efficiency.Score.Should().BeApproximately(100.0, 0.5);
        result.Efficiency.Status.Should().Be("scored");
    }

    [Fact]
    public void Calculate_InefficientIceVehicle_ReturnsZeroEfficiencyScore()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m, year: 2020, mileageKm: 60000, defectCount: 0,
            market: null,
            efficiency: new FuelEconomyEfficiencyResult(LitersPer100Km: 15.0, KWhPer100Km: null),
            recalls: null, complaints: null, safetyRating: null);

        // (12 - 15) / 6 clamped to 0 = 0
        result.Efficiency.Score.Should().BeApproximately(0.0, 0.5);
    }

    [Fact]
    public void Calculate_EfficientEv_ReturnsHighEfficiencyScore()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m, year: 2020, mileageKm: 60000, defectCount: 0,
            market: null,
            efficiency: new FuelEconomyEfficiencyResult(LitersPer100Km: null, KWhPer100Km: 15.0),
            recalls: null, complaints: null, safetyRating: null);

        // (25 - 15) / 10 * 100 = 100
        result.Efficiency.Score.Should().BeApproximately(100.0, 0.5);
    }

    [Fact]
    public void Calculate_NullEfficiency_EfficiencyFactorIsMissing()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m, year: 2020, mileageKm: 60000, defectCount: 0,
            market: null, efficiency: null, recalls: null, complaints: null, safetyRating: null);

        result.Efficiency.Status.Should().Be("missing");
        result.MissingFactors.Should().Contain("Efficiency");
    }

    // ── Reliability factor ────────────────────────────────────────────────────

    [Fact]
    public void Calculate_NoRecallsOrComplaints_NoSafetyRating_ReliabilityScore100()
    {
        // Unrated branch: 0.60 * 100 + 0.40 * 100 = 100
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m, year: 2020, mileageKm: 60000, defectCount: 0,
            market: null, efficiency: null,
            recalls: new NhtsaRecallsResult(0),
            complaints: new NhtsaComplaintsResult(0),
            safetyRating: null);

        result.Reliability.Score.Should().BeApproximately(100.0, 0.5);
        result.Reliability.Status.Should().Be("scored");
    }

    [Fact]
    public void Calculate_PerfectSafetyRating_BoostsReliabilityScore()
    {
        // 0 recalls, 0 complaints, 5 stars → all sub-scores = 100 → overall = 100
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m, year: 2020, mileageKm: 60000, defectCount: 0,
            market: null, efficiency: null,
            recalls: new NhtsaRecallsResult(0),
            complaints: new NhtsaComplaintsResult(0),
            safetyRating: new NhtsaSafetyRatingResult(5));

        result.Reliability.Score.Should().BeApproximately(100.0, 0.5);
    }

    [Fact]
    public void Calculate_OneStar_ZeroRecalls_ZeroComplaints_SafetyDragsScoreDown()
    {
        // safetyScore = 0, recallScore = 100, complaintScore = 100
        // 0.40*100 + 0.20*100 + 0.40*0 = 60
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m, year: 2020, mileageKm: 60000, defectCount: 0,
            market: null, efficiency: null,
            recalls: new NhtsaRecallsResult(0),
            complaints: new NhtsaComplaintsResult(0),
            safetyRating: new NhtsaSafetyRatingResult(1));

        result.Reliability.Score.Should().BeApproximately(60.0, 0.5);
    }

    [Fact]
    public void Calculate_TenRecalls_NoComplaints_NoRating_RecallPenaltyApplied()
    {
        // recallScore = 100 - clamp(10/10)*100 = 0
        // complaintScore = 100
        // unrated: 0.60*0 + 0.40*100 = 40
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m, year: 2020, mileageKm: 60000, defectCount: 0,
            market: null, efficiency: null,
            recalls: new NhtsaRecallsResult(10),
            complaints: new NhtsaComplaintsResult(0),
            safetyRating: null);

        result.Reliability.Score.Should().BeApproximately(40.0, 0.5);
    }

    [Fact]
    public void Calculate_ToyotaCamry2020_ReliabilityScoreApprox78()
    {
        // 3 recalls, 256 complaints, 5 stars
        // recallScore = 100 - 30 = 70
        // complaintScore = 100 - (256/500)*100 = 48.8
        // safetyScore = 100
        // 0.40*70 + 0.20*48.8 + 0.40*100 = 28 + 9.76 + 40 = 77.76
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m, year: 2020, mileageKm: 60000, defectCount: 0,
            market: null, efficiency: null,
            recalls: new NhtsaRecallsResult(3),
            complaints: new NhtsaComplaintsResult(256),
            safetyRating: new NhtsaSafetyRatingResult(5));

        result.Reliability.Score.Should().BeApproximately(77.76, 1.0);
    }

    [Fact]
    public void Calculate_MaxedOutRecallsAndComplaints_WorstRating_ReliabilityScore0()
    {
        // recallScore = 0, complaintScore = 0, safetyScore = 0
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m, year: 2020, mileageKm: 60000, defectCount: 0,
            market: null, efficiency: null,
            recalls: new NhtsaRecallsResult(20),
            complaints: new NhtsaComplaintsResult(1000),
            safetyRating: new NhtsaSafetyRatingResult(1));

        result.Reliability.Score.Should().BeApproximately(0.0, 0.5);
    }

    [Fact]
    public void Calculate_NullReliabilityInputs_ReliabilityFactorIsMissing()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m, year: 2020, mileageKm: 60000, defectCount: 0,
            market: null, efficiency: null, recalls: null, complaints: null, safetyRating: null);

        result.Reliability.Status.Should().Be("missing");
        result.Reliability.Score.Should().Be(50);
        result.MissingFactors.Should().Contain("Reliability");
    }

    [Fact]
    public void Calculate_OnlyRecallsProvided_ScoredNotMissing()
    {
        // Partial data (only recalls) → still scored, complaints treated as 0
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m, year: 2020, mileageKm: 60000, defectCount: 0,
            market: null, efficiency: null,
            recalls: new NhtsaRecallsResult(2),
            complaints: null,
            safetyRating: null);

        result.Reliability.Status.Should().Be("scored");
    }

    // ── Mileage factor ────────────────────────────────────────────────────────

    [Fact]
    public void Calculate_LowMileageForAge_HighMileageScore()
    {
        var carYear = DateTime.UtcNow.Year - 5;
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m, year: carYear, mileageKm: 30000, defectCount: 0,
            market: null, efficiency: null, recalls: null, complaints: null, safetyRating: null);

        result.Mileage.Status.Should().Be("scored");
        result.Mileage.Score.Should().BeGreaterThan(50);
    }

    // ── Condition factor ──────────────────────────────────────────────────────

    [Fact]
    public void Calculate_ZeroDefects_ConditionScore100()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m, year: 2020, mileageKm: 60000, defectCount: 0,
            market: null, efficiency: null, recalls: null, complaints: null, safetyRating: null);

        result.Condition.Score.Should().Be(100);
        result.Condition.Status.Should().Be("scored");
    }

    [Fact]
    public void Calculate_ThreeDefects_ConditionScore40()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m, year: 2020, mileageKm: 60000, defectCount: 3,
            market: null, efficiency: null, recalls: null, complaints: null, safetyRating: null);

        result.Condition.Score.Should().Be(40);
    }

    [Fact]
    public void Calculate_FiveOrMoreDefects_ConditionScoreZero()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m, year: 2020, mileageKm: 60000, defectCount: 7,
            market: null, efficiency: null, recalls: null, complaints: null, safetyRating: null);

        result.Condition.Score.Should().Be(0);
    }

    // ── Overall ───────────────────────────────────────────────────────────────

    [Fact]
    public void Calculate_AllExternalDataMissing_ThreeMissingFactors()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m, year: 2020, mileageKm: 60000, defectCount: 0,
            market: null, efficiency: null, recalls: null, complaints: null, safetyRating: null);

        result.HasMissingFactors.Should().BeTrue();
        result.MissingFactors.Should().HaveCount(3);
        result.MissingFactors.Should().Contain("Market Value");
        result.MissingFactors.Should().Contain("Efficiency");
        result.MissingFactors.Should().Contain("Reliability");
        result.Mileage.Status.Should().Be("scored");
        result.OverallScore.Should().BeGreaterThan(0).And.BeLessThan(100);
    }

    [Fact]
    public void Calculate_AllDataPresent_OverallScoreAbove70()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 8000m, year: 2020, mileageKm: 30000, defectCount: 0,
            market: new CardogMarketResult(MedianPrice: 10000m, TotalListings: 50),
            efficiency: new FuelEconomyEfficiencyResult(LitersPer100Km: 6.0, KWhPer100Km: null),
            recalls: new NhtsaRecallsResult(0),
            complaints: new NhtsaComplaintsResult(0),
            safetyRating: new NhtsaSafetyRatingResult(5));

        result.OverallScore.Should().BeGreaterThan(70);
        result.HasMissingFactors.Should().BeFalse();
    }
}
