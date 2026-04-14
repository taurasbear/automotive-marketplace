namespace Automotive.Marketplace.Domain.Entities;

public class DrivetrainTranslation : BaseEntity
{
    public Guid DrivetrainId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public virtual Drivetrain Drivetrain { get; set; } = null!;
}
