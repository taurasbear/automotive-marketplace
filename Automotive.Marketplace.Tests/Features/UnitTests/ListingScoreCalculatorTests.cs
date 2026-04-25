using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingScore;
using Automotive.Marketplace.Application.Interfaces.Services;
using FluentAssertions;

namespace Automotive.Marketplace.Tests.Features.UnitTests;

public class ListingScoreCalculatorTests
{
    [Fact]
    public void Calculate_ListingBelowMarket_ReturnsHighValueScore()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 8000m,
            year: 2020,
            mileageKm: 60000,
            market: new CardogMarketResult(MedianPrice: 10000m, TotalListings: 50),
            efficiency: null,
            reliability: null);

        // (10000 - 8000) / 10000 = 0.20 → (0.20 + 0.5) / 0.8 * 100 = 87.5
        result.Value.Score.Should().BeApproximately(87.5, 0.5);
        result.Value.Status.Should().Be("scored");
    }

    [Fact]
    public void Calculate_ListingAtMarket_ReturnsApprox62Score()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 60000,
            market: new CardogMarketResult(MedianPrice: 10000m, TotalListings: 50),
            efficiency: null,
            reliability: null);

        // (0 + 0.5) / 0.8 * 100 = 62.5
        result.Value.Score.Should().BeApproximately(62.5, 0.5);
    }

    [Fact]
    public void Calculate_NullMarket_ValueFactorIsMissing()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 8000m,
            year: 2020,
            mileageKm: 60000,
            market: null,
            efficiency: null,
            reliability: null);

        result.Value.Status.Should().Be("missing");
        result.Value.Score.Should().Be(50);
        result.HasMissingFactors.Should().BeTrue();
        result.MissingFactors.Should().Contain("Market Value");
    }

    [Fact]
    public void Calculate_EfficientIceVehicle_ReturnsHighEfficiencyScore()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 60000,
            market: null,
            efficiency: new CardogEfficiencyResult(LitersPer100Km: 6.0, KWhPer100Km: null),
            reliability: null);

        // (12 - 6) / 6 * 100 = 100
        result.Efficiency.Score.Should().BeApproximately(100.0, 0.5);
        result.Efficiency.Status.Should().Be("scored");
    }

    [Fact]
    public void Calculate_InefficientIceVehicle_ReturnsZeroEfficiencyScore()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 60000,
            market: null,
            efficiency: new CardogEfficiencyResult(LitersPer100Km: 15.0, KWhPer100Km: null),
            reliability: null);

        // (12 - 15) / 6 clamped to 0 = 0
        result.Efficiency.Score.Should().BeApproximately(0.0, 0.5);
    }

    [Fact]
    public void Calculate_EfficientEv_ReturnsHighEfficiencyScore()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 60000,
            market: null,
            efficiency: new CardogEfficiencyResult(LitersPer100Km: null, KWhPer100Km: 15.0),
            reliability: null);

        // (25 - 15) / 10 * 100 = 100
        result.Efficiency.Score.Should().BeApproximately(100.0, 0.5);
    }

    [Fact]
    public void Calculate_NullEfficiency_EfficiencyFactorIsMissing()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 60000,
            market: null,
            efficiency: null,
            reliability: null);

        result.Efficiency.Status.Should().Be("missing");
        result.MissingFactors.Should().Contain("Efficiency");
    }

    [Fact]
    public void Calculate_NoRecallsOrComplaints_ReliabilityScore100()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 60000,
            market: null,
            efficiency: null,
            reliability: new CardogReliabilityResult(RecallCount: 0, ComplaintCrashes: 0, ComplaintInjuries: 0));

        result.Reliability.Score.Should().BeApproximately(100.0, 0.5);
        result.Reliability.Status.Should().Be("scored");
    }

    [Fact]
    public void Calculate_NullReliability_ReliabilityFactorIsMissing()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 60000,
            market: null,
            efficiency: null,
            reliability: null);

        result.Reliability.Status.Should().Be("missing");
        result.Reliability.Score.Should().Be(50);
        result.MissingFactors.Should().Contain("Reliability");
    }

    [Fact]
    public void Calculate_LowMileageForAge_HighMileageScore()
    {
        var carYear = DateTime.UtcNow.Year - 5; // Always 5 years old
        var expectedKm = 5 * 15000; // 75000 km expected
        var actualKm = 30000; // below expected

        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: carYear,
            mileageKm: actualKm,
            market: null,
            efficiency: null,
            reliability: null);

        result.Mileage.Status.Should().Be("scored");
        result.Mileage.Score.Should().BeGreaterThan(50); // (75000 - 30000) / 75000 * 100 = 60
    }

    [Fact]
    public void Calculate_AllExternalDataMissing_ThreeMissingFactors()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 10000m,
            year: 2020,
            mileageKm: 60000,
            market: null,
            efficiency: null,
            reliability: null);

        result.HasMissingFactors.Should().BeTrue();
        result.MissingFactors.Should().HaveCount(3);
        result.MissingFactors.Should().Contain("Market Value");
        result.MissingFactors.Should().Contain("Efficiency");
        result.MissingFactors.Should().Contain("Reliability");
        // Only mileage is scored, so overall is based solely on mileage
        result.Mileage.Status.Should().Be("scored");
        result.OverallScore.Should().BeGreaterThan(0).And.BeLessThan(100);
    }

    [Fact]
    public void Calculate_OverallScore_IsWeightedAverage()
    {
        var result = ListingScoreCalculator.Calculate(
            listingPrice: 8000m,
            year: 2020,
            mileageKm: 30000,
            market: new CardogMarketResult(MedianPrice: 10000m, TotalListings: 50),
            efficiency: new CardogEfficiencyResult(LitersPer100Km: 6.0, KWhPer100Km: null),
            reliability: new CardogReliabilityResult(RecallCount: 0, ComplaintCrashes: 0, ComplaintInjuries: 0));

        result.OverallScore.Should().BeGreaterThan(70);
        result.HasMissingFactors.Should().BeFalse();
    }
}
