namespace Automotive.Marketplace.Domain.Entities;

public class Transmission : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<TransmissionTranslation> Translations { get; set; } = [];
}
