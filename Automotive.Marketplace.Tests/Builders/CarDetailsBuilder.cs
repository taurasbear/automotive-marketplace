using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Tests.Builders;

public class CarDetailsBuilder
{
    private readonly Faker<CarDetails> _faker;

    public CarDetailsBuilder()
    {
        _faker = new Faker<CarDetails>()
            .RuleFor(carDetails => carDetails.Id, faker => faker.Random.Guid())
            .RuleFor(carDetails => carDetails.Vin, faker => faker.Vehicle.Vin())
            .RuleFor(carDetails => carDetails.Colour, faker => faker.Commerce.Color())
            .RuleFor(carDetails => carDetails.Used, faker => faker.Random.Bool())
            .RuleFor(carDetails => carDetails.Power, faker => faker.Random.Int(40, 500))
            .RuleFor(carDetails => carDetails.EngineSize, faker => faker.Random.Int(800, 3000))
            .RuleFor(carDetails => carDetails.Mileage, faker => faker.Random.Int(10000, 400000))
            .RuleFor(carDetails => carDetails.IsSteeringWheelRight, faker => faker.Random.Bool())
            .RuleFor(carDetails => carDetails.EngineSize, faker => faker.Random.Int(800, 3000));
    }

    public CarDetailsBuilder WithUsed(bool isUsed)
    {
        _faker.RuleFor(carDetails => carDetails.Used, isUsed);
        return this;
    }

    public CarDetailsBuilder WithCar(Guid carId)
    {
        _faker.RuleFor(car => car.CarId, carId);
        return this;
    }

    public CarDetailsBuilder With<T>(Expression<Func<CarDetails, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public CarDetails Build() => _faker.Generate();

    public List<CarDetails> Build(int count) => _faker.Generate(count);
}