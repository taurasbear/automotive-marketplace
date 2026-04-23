using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Bogus;
using System.Linq.Expressions;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class OfferBuilder
{
    private readonly Faker<Offer> _faker;

    public OfferBuilder()
    {
        _faker = new Faker<Offer>()
            .RuleFor(o => o.Id, f => f.Random.Guid())
            .RuleFor(o => o.ConversationId, f => f.Random.Guid())
            .RuleFor(o => o.InitiatorId, f => f.Random.Guid())
            .RuleFor(o => o.Amount, f => f.Random.Decimal(1000, 10000))
            .RuleFor(o => o.Status, OfferStatus.Pending)
            .RuleFor(o => o.ExpiresAt, _ => DateTime.UtcNow.AddHours(48))
            .RuleFor(o => o.CreatedAt, _ => DateTime.UtcNow)
            .RuleFor(o => o.CreatedBy, f => f.Random.Guid().ToString());
    }

    public OfferBuilder WithConversation(Guid conversationId)
    {
        _faker.RuleFor(o => o.ConversationId, conversationId);
        return this;
    }

    public OfferBuilder WithInitiator(Guid initiatorId)
    {
        _faker.RuleFor(o => o.InitiatorId, initiatorId);
        return this;
    }

    public OfferBuilder WithAmount(decimal amount)
    {
        _faker.RuleFor(o => o.Amount, amount);
        return this;
    }

    public OfferBuilder WithStatus(OfferStatus status)
    {
        _faker.RuleFor(o => o.Status, status);
        return this;
    }

    public OfferBuilder AsExpired()
    {
        _faker.RuleFor(o => o.ExpiresAt, _ => DateTime.UtcNow.AddHours(-1));
        return this;
    }

    public OfferBuilder WithParent(Guid parentOfferId)
    {
        _faker.RuleFor(o => o.ParentOfferId, parentOfferId);
        return this;
    }

    public OfferBuilder With<T>(Expression<Func<Offer, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public Offer Build() => _faker.Generate();
}
