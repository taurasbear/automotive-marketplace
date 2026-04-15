using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class BodyTypeBuilder
{
    private readonly Faker<BodyType> _faker;

    public BodyTypeBuilder()
    {
        _faker = new Faker<BodyType>()
            .RuleFor(b => b.Id, faker => faker.Random.Guid())
            .RuleFor(b => b.Name, faker => faker.Lorem.Word());
    }

    public BodyType Build() => _faker.Generate();
}
