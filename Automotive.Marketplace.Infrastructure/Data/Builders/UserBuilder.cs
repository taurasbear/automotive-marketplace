using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class UserBuilder
{
    private readonly Faker<User> _faker;

    public UserBuilder()
    {
        _faker = new Faker<User>()
            .RuleFor(make => make.Id, faker => faker.Random.Guid())
            .RuleFor(make => make.Username, faker => faker.Internet.UserName())
            .RuleFor(make => make.Email, faker => faker.Internet.Email())
            .RuleFor(make => make.HashedPassword, faker => faker.Internet.Password());
    }

    public UserBuilder With<T>(Expression<Func<User, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public User Build() => _faker.Generate();

    public List<User> Build(int count) => _faker.Generate(count);
}