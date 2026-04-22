using Automotive.Marketplace.Domain.Entities;
using Bogus;
using System.Linq.Expressions;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class UserListingLikeBuilder
{
    private readonly Faker<UserListingLike> _faker;

    public UserListingLikeBuilder()
    {
        _faker = new Faker<UserListingLike>()
            .RuleFor(l => l.Id, f => f.Random.Guid())
            .RuleFor(l => l.UserId, f => f.Random.Guid())
            .RuleFor(l => l.ListingId, f => f.Random.Guid())
            .RuleFor(l => l.CreatedAt, _ => DateTime.UtcNow)
            .RuleFor(l => l.CreatedBy, f => f.Random.Guid().ToString());
    }

    public UserListingLikeBuilder WithUser(Guid userId)
    {
        _faker.RuleFor(l => l.UserId, userId);
        return this;
    }

    public UserListingLikeBuilder WithListing(Guid listingId)
    {
        _faker.RuleFor(l => l.ListingId, listingId);
        return this;
    }

    public UserListingLikeBuilder With<T>(Expression<Func<UserListingLike, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public UserListingLike Build() => _faker.Generate();
}
