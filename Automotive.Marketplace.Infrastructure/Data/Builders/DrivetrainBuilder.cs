using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class DrivetrainBuilder
{
    private readonly Faker<Drivetrain> _faker;

    public DrivetrainBuilder()
    {
        _faker = new Faker<Drivetrain>()
            .RuleFor(d => d.Id, faker => faker.Random.Guid())
            .RuleFor(d => d.Name, faker => faker.Lorem.Word());
    }

    public Drivetrain Build() => _faker.Generate();
}
