namespace Automotive.Marketplace.Domain.Entities;

public class Make : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public DateOnly FirstAppearanceDate { get; set; }

    public decimal TotalRevenue { get; set; }

    public virtual ICollection<Model> Models { get; set; } = [];
}
