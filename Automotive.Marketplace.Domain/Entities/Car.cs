namespace Automotive.Marketplace.Domain.Entities
{
    public class Car : BaseEntity
    {
        public DateTime Year { get; set; }

        public Fuel Fuel { get; set; }

        public Transmission Transmission { get; set; }
    }
}
