using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class ConversationBuilder
{
    private readonly Faker<Conversation> _faker;

    public ConversationBuilder()
    {
        _faker = new Faker<Conversation>()
            .RuleFor(c => c.Id, f => f.Random.Guid())
            .RuleFor(c => c.BuyerId, f => f.Random.Guid())
            .RuleFor(c => c.ListingId, f => f.Random.Guid())
            .RuleFor(c => c.CreatedAt, f => f.Date.Past(1).ToUniversalTime())
            .RuleFor(c => c.LastMessageAt, (f, c) => f.Date.Between(c.CreatedAt, DateTime.UtcNow).ToUniversalTime())
            .RuleFor(c => c.CreatedBy, f => f.Random.Guid().ToString());
    }

    public ConversationBuilder WithBuyer(Guid buyerId)
    {
        _faker.RuleFor(c => c.BuyerId, buyerId);
        return this;
    }

    public ConversationBuilder WithListing(Guid listingId)
    {
        _faker.RuleFor(c => c.ListingId, listingId);
        return this;
    }

    public ConversationBuilder With<T>(Expression<Func<Conversation, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public Conversation Build() => _faker.Generate();
}
