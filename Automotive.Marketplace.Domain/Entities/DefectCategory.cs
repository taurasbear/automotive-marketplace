namespace Automotive.Marketplace.Domain.Entities;

public class DefectCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<DefectCategoryTranslation> Translations { get; set; } = [];
}
