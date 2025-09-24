namespace Automotive.Marketplace.Domain.Entities;

public class Model : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public DateOnly FirstAppearanceDate { get; set; }

    public bool IsDiscontinued { get; set; }

    public Guid MakeId { get; set; }

    public virtual Make Make { get; set; } = null!;

    public virtual ICollection<Car> Cars { get; set; } = [];
}
