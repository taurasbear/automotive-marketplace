namespace Automotive.Marketplace.Domain.Entities;

public class DefectCategoryTranslation : BaseEntity
{
    public Guid DefectCategoryId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public virtual DefectCategory DefectCategory { get; set; } = null!;
}
