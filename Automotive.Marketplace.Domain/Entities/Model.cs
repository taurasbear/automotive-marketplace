namespace Automotive.Marketplace.Domain.Entities
{
    using System.Collections.ObjectModel;

    public class Model : BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public Guid MakeId { get; set; }

        public Make Make { get; set; } = null!;

        public ICollection<Car> Cars { get; set; } = new Collection<Car>();
    }
}
