namespace Automotive.Marketplace.Domain.Entities
{
    public class CarDetails : BaseEntity
    {
        public string Vin { get; set; } = string.Empty;

        public string Colour { get; set; } = string.Empty;

        public bool Used { get; set; }

        public int Power { get; set; }

        public int EngineSize { get; set; }

        public int Mileage { get; set; }

        public bool IsSteeringWheelRight { get; set; }

        public Guid CarId { get; set; }

        public Car Car { get; set; } = null!;

        public Listing Listing { get; set; } = null!;
    }
}
