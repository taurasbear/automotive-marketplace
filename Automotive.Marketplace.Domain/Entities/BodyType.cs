namespace Automotive.Marketplace.Domain.Entities;

public class BodyType : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<BodyTypeTranslation> Translations { get; set; } = [];
}
