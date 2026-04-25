namespace Automotive.Marketplace.Domain.Entities;

public class UserPreferences : BaseEntity
{
    public Guid UserId { get; set; }

    public double ValueWeight { get; set; } = 0.30;
    public double EfficiencyWeight { get; set; } = 0.25;
    public double ReliabilityWeight { get; set; } = 0.25;
    public double MileageWeight { get; set; } = 0.20;

    public bool AutoGenerateAiSummary { get; set; }

    public virtual User User { get; set; } = null!;
}
