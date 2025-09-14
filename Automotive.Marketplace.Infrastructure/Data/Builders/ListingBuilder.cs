using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class ListingBuilder
{
    private readonly Faker<Listing> _faker;

    public ListingBuilder()
    {
        _faker = new Faker<Listing>()
            .RuleFor(listing => listing.Id, faker => faker.Random.Guid())
            .RuleFor(listing => listing.Price, faker => faker.Random.Decimal(100, 50000))
            .RuleFor(listing => listing.Description, faker => faker.Lorem.Sentences(2))
            .RuleFor(listing => listing.City, faker => faker.Address.City())
            .RuleFor(listing => listing.Status, Status.Available)
            .RuleFor(listing => listing.Id, faker => faker.Random.Guid())
            .RuleFor(listing => listing.Vin, faker => faker.Vehicle.Vin())
            .RuleFor(listing => listing.Colour, faker => faker.Commerce.Color())
            .RuleFor(listing => listing.IsUsed, faker => faker.Random.Bool())
            .RuleFor(listing => listing.Power, faker => faker.Random.Int(40, 500))
            .RuleFor(listing => listing.EngineSize, faker => faker.Random.Int(800, 3000))
            .RuleFor(listing => listing.Mileage, faker => faker.Random.Int(10000, 400000))
            .RuleFor(listing => listing.IsSteeringWheelRight, faker => faker.Random.Bool())
            .RuleFor(listing => listing.EngineSize, faker => faker.Random.Int(800, 3000));
    }

    public ListingBuilder WithPrice(decimal price)
    {
        _faker.RuleFor(listing => listing.Price, price);
        return this;
    }

    public ListingBuilder Withlisting(Guid listingId)
    {
        _faker.RuleFor(listing => listing.Id, listingId);
        return this;
    }

    public ListingBuilder WithSeller(Guid sellerId)
    {
        _faker.RuleFor(listing => listing.SellerId, sellerId);
        return this;
    }

    public ListingBuilder WithUsed(bool isUsed)
    {
        _faker.RuleFor(listing => listing.IsUsed, isUsed);
        return this;
    }

    public ListingBuilder WithCar(Guid carId)
    {
        _faker.RuleFor(listing => listing.CarId, carId);
        return this;
    }

    public ListingBuilder With<T>(Expression<Func<Listing, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public Listing Build() => _faker.Generate();

    public List<Listing> Build(int count) => _faker.Generate(count);
}