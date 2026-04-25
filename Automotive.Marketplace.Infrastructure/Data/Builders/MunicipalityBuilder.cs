using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class MunicipalityBuilder
{
    private readonly Faker<Municipality> _faker;

    public MunicipalityBuilder()
    {
        _faker = new Faker<Municipality>()
            .RuleFor(m => m.Id, f => f.Random.Guid())
            .RuleFor(m => m.Name, f => f.Address.City())
            .RuleFor(m => m.SyncedAt, f => f.Date.PastOffset().UtcDateTime)
            .RuleFor(m => m.CreatedAt, f => f.Date.PastOffset().UtcDateTime)
            .RuleFor(m => m.CreatedBy, f => f.Internet.UserName());
    }

    public Municipality Build() => _faker.Generate();
}
