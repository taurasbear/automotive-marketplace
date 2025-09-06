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
            .RuleFor(listing => listing.Status, Status.Available);
    }

    public ListingBuilder WithPrice(decimal price)
    {
        _faker.RuleFor(listing => listing.Price, price);
        return this;
    }

    public ListingBuilder WithCarDetails(Guid carDetailsId)
    {
        _faker.RuleFor(listing => listing.CarDetailsId, carDetailsId);
        return this;
    }

    public ListingBuilder WithSeller(Guid sellerId)
    {
        _faker.RuleFor(listing => listing.SellerId, sellerId);
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