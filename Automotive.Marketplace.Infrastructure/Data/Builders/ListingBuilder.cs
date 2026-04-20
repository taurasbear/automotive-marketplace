using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class ListingBuilder
{
    private readonly Faker<Listing> _faker;

    public ListingBuilder()
    {
        _faker = new Faker<Listing>()
            .RuleFor(listing => listing.Id, f => f.Random.Guid())
            .RuleFor(listing => listing.Price, f => f.Random.Decimal(100, 50000))
            .RuleFor(listing => listing.Description, f => f.Lorem.Sentences(2))
            .RuleFor(listing => listing.City, f => f.Address.City())
            .RuleFor(listing => listing.Status, Status.Available)
            .RuleFor(listing => listing.Vin, f => f.Vehicle.Vin())
            .RuleFor(listing => listing.Colour, f => f.Commerce.Color())
            .RuleFor(listing => listing.IsUsed, f => f.Random.Bool())
            .RuleFor(listing => listing.Mileage, f => f.Random.Int(10000, 400000))
            .RuleFor(listing => listing.IsSteeringWheelRight, f => f.Random.Bool());
    }

    public ListingBuilder WithPrice(decimal price)
    {
        _faker.RuleFor(listing => listing.Price, price);
        return this;
    }

    public ListingBuilder WithListing(Guid listingId)
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

    public ListingBuilder WithVariant(Guid variantId)
    {
        _faker.RuleFor(listing => listing.VariantId, variantId);
        return this;
    }

    public ListingBuilder WithDrivetrain(Guid drivetrainId)
    {
        _faker.RuleFor(listing => listing.DrivetrainId, drivetrainId);
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