using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class VariantBuilder
{
    private readonly Faker<Variant> _faker;

    public VariantBuilder()
    {
        _faker = new Faker<Variant>()
            .RuleFor(v => v.Id, f => f.Random.Guid())
            .RuleFor(v => v.PowerKw, f => f.Random.Int(40, 500))
            .RuleFor(v => v.EngineSizeMl, f => f.Random.Int(800, 6000))
            .RuleFor(v => v.DoorCount, f => f.Random.Int(2, 6))
            .RuleFor(v => v.IsCustom, _ => false);
    }

    public VariantBuilder WithModel(Guid modelId)
    {
        _faker.RuleFor(v => v.ModelId, modelId);
        return this;
    }

    public VariantBuilder WithFuel(Guid fuelId)
    {
        _faker.RuleFor(v => v.FuelId, fuelId);
        return this;
    }

    public VariantBuilder WithTransmission(Guid transmissionId)
    {
        _faker.RuleFor(v => v.TransmissionId, transmissionId);
        return this;
    }

    public VariantBuilder WithBodyType(Guid bodyTypeId)
    {
        _faker.RuleFor(v => v.BodyTypeId, bodyTypeId);
        return this;
    }

    public VariantBuilder With<T>(Expression<Func<Variant, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public Variant Build() => _faker.Generate();

    public List<Variant> Build(int count) => _faker.Generate(count);
}
