namespace Automotive.Marketplace.Domain.Entities;

public class Fuel : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<FuelTranslation> Translations { get; set; } = [];
}
