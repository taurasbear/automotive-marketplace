namespace Automotive.Marketplace.Domain.Entities;

public class UserPreferences : BaseEntity
{
    public Guid UserId { get; set; }

    public double ValueWeight { get; set; } = 0.26;
    public double EfficiencyWeight { get; set; } = 0.21;
    public double ReliabilityWeight { get; set; } = 0.21;
    public double MileageWeight { get; set; } = 0.17;
    public double ConditionWeight { get; set; } = 0.15;

    public bool AutoGenerateAiSummary { get; set; }
    public bool EnableVehicleScoring { get; set; }
    public bool HasCompletedQuiz { get; set; } = false;
    public bool EnableMarketPriceApi { get; set; } = false;

    public virtual User User { get; set; } = null!;
}
