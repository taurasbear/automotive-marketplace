namespace Automotive.Marketplace.Domain.Entities;

using System.Collections.ObjectModel;

public class Make : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<Model> Models { get; set; } = new Collection<Model>();
}
