using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class FuelBuilder
{
    private readonly Faker<Fuel> _faker;

    public FuelBuilder()
    {
        _faker = new Faker<Fuel>()
            .RuleFor(f => f.Id, faker => faker.Random.Guid())
            .RuleFor(f => f.Name, faker => faker.Lorem.Word());
    }

    public Fuel Build() => _faker.Generate();
}
