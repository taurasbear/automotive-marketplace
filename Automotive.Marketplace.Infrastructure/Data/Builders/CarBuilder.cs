using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class CarBuilder
{
    private readonly Faker<Car> _faker;

    public CarBuilder()
    {
        _faker = new Faker<Car>()
            .RuleFor(car => car.Id, faker => faker.Random.Guid())
            .RuleFor(
                car => car.Year,
                faker => DateTime.SpecifyKind(
                    faker.Date.Between(new DateTime(1980, 1, 1), new DateTime(2025, 1, 1)),
                    DateTimeKind.Utc))
            .RuleFor(car => car.Fuel, faker => faker.Random.Enum<Fuel>())
            .RuleFor(car => car.Transmission, faker => faker.Random.Enum<Transmission>())
            .RuleFor(car => car.BodyType, faker => faker.Random.Enum<BodyType>())
            .RuleFor(car => car.Drivetrain, faker => faker.Random.Enum<Drivetrain>())
            .RuleFor(car => car.DoorCount, faker => faker.Random.Int(1, 8));
    }

    public CarBuilder WithYear(DateTime year)
    {
        _faker.RuleFor(car => car.Year, year);
        return this;
    }

    public CarBuilder WithModel(Guid modelId)
    {
        _faker.RuleFor(car => car.ModelId, modelId);
        return this;
    }

    public CarBuilder With<T>(Expression<Func<Car, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public Car Build() => _faker.Generate();

    public List<Car> Build(int count) => _faker.Generate(count);
}