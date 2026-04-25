namespace Automotive.Marketplace.Application.Features.UserPreferencesFeatures.GetUserPreferences;

public class GetUserPreferencesResponse
{
    public double ValueWeight { get; init; }
    public double EfficiencyWeight { get; init; }
    public double ReliabilityWeight { get; init; }
    public double MileageWeight { get; init; }
    public bool AutoGenerateAiSummary { get; init; }
    public bool HasPreferences { get; init; }
}
