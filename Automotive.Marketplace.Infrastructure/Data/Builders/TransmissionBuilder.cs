using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class TransmissionBuilder
{
    private readonly Faker<Transmission> _faker;

    public TransmissionBuilder()
    {
        _faker = new Faker<Transmission>()
            .RuleFor(t => t.Id, faker => faker.Random.Guid())
            .RuleFor(t => t.Name, faker => faker.Lorem.Word());
    }

    public Transmission Build() => _faker.Generate();
}
