namespace Automotive.Marketplace.Domain.Entities;

public class BodyTypeTranslation : BaseEntity
{
    public Guid BodyTypeId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public virtual BodyType BodyType { get; set; } = null!;
}
