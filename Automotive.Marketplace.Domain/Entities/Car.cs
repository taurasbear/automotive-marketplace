namespace Automotive.Marketplace.Domain.Entities
{
    using Automotive.Marketplace.Domain.Enums;
    using System.Collections.ObjectModel;

    public class Car : BaseEntity
    {
        public DateTime Year { get; set; }

        public Fuel Fuel { get; set; }

        public Transmission Transmission { get; set; }

        public BodyType BodyType { get; set; }

        public Drivetrain Drivetrain { get; set; }

        public int DoorCount { get; set; }

        public Guid ModelId { get; set; }

        public Model Model { get; set; } = new Model();

        public ICollection<CarDetails> CarDetails { get; set; } = new Collection<CarDetails>();
    }
}
