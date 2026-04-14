namespace Automotive.Marketplace.Domain.Entities;

public class Drivetrain : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<DrivetrainTranslation> Translations { get; set; } = [];
}
