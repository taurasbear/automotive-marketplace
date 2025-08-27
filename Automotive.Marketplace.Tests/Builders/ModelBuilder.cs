using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Tests.Builders;

public class ModelBuilder
{
    private readonly Faker<Model> _faker;

    public ModelBuilder()
    {
        _faker = new Faker<Model>()
            .RuleFor(model => model.Id, faker => faker.Random.Guid())
            .RuleFor(model => model.Name, faker => faker.Vehicle.Model());
    }

    public ModelBuilder WithMake(Guid makeId)
    {
        _faker.RuleFor(model => model.MakeId, makeId);
        return this;
    }

    public ModelBuilder With<T>(Expression<Func<Model, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public Model Build() => _faker.Generate();

    public List<Model> Build(int count) => _faker.Generate(count);
}