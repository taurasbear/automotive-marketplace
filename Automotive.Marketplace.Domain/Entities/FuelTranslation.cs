namespace Automotive.Marketplace.Domain.Entities;

public class FuelTranslation : BaseEntity
{
    public Guid FuelId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public virtual Fuel Fuel { get; set; } = null!;
}
