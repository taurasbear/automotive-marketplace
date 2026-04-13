namespace Automotive.Marketplace.Domain.Entities;

public class Make : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public virtual ICollection<Model> Models { get; set; } = [];
}
