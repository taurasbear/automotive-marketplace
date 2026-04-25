using MediatR;

namespace Automotive.Marketplace.Application.Features.UserPreferencesFeatures.UpsertUserPreferences;

public class UpsertUserPreferencesCommand : IRequest
{
    public Guid UserId { get; set; }
    public double ValueWeight { get; set; }
    public double EfficiencyWeight { get; set; }
    public double ReliabilityWeight { get; set; }
    public double MileageWeight { get; set; }
    public bool AutoGenerateAiSummary { get; set; }
}
