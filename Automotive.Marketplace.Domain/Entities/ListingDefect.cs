namespace Automotive.Marketplace.Domain.Entities;

public class ListingDefect : BaseEntity
{
    public Guid ListingId { get; set; }

    public Guid? DefectCategoryId { get; set; }

    public string? CustomName { get; set; }

    public virtual Listing Listing { get; set; } = null!;

    public virtual DefectCategory? DefectCategory { get; set; }

    public virtual ICollection<Image> Images { get; set; } = [];
}
