using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class MakeBuilder
{
    private readonly Faker<Make> _faker;

    public MakeBuilder()
    {
        _faker = new Faker<Make>()
            .RuleFor(make => make.Id, faker => faker.Random.Guid())
            .RuleFor(
                make => make.FirstAppearanceDate,
                faker => faker.Date.BetweenDateOnly(new DateOnly(1970, 1, 1), DateOnly.FromDateTime(DateTime.UtcNow)))
            .RuleFor(make => make.TotalRevenue, faker => faker.Random.Decimal(99999, 9999999999))
            .RuleFor(make => make.Name, faker => faker.Vehicle.Manufacturer());
    }

    public MakeBuilder With<T>(Expression<Func<Make, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public Make Build() => _faker.Generate();

    public List<Make> Build(int count) => _faker.Generate(count);
}