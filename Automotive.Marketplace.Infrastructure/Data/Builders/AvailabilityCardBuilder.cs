using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class AvailabilityCardBuilder
{
    private readonly Faker<AvailabilityCard> _faker;

    public AvailabilityCardBuilder()
    {
        _faker = new Faker<AvailabilityCard>()
            .RuleFor(a => a.Id, f => f.Random.Guid())
            .RuleFor(a => a.ConversationId, f => f.Random.Guid())
            .RuleFor(a => a.InitiatorId, f => f.Random.Guid())
            .RuleFor(a => a.Status, AvailabilityCardStatus.Pending)
            .RuleFor(a => a.ExpiresAt, _ => DateTime.UtcNow.AddHours(48))
            .RuleFor(a => a.CreatedAt, _ => DateTime.UtcNow)
            .RuleFor(a => a.CreatedBy, f => f.Random.Guid().ToString());
    }

    public AvailabilityCardBuilder WithConversation(Guid conversationId)
    {
        _faker.RuleFor(a => a.ConversationId, conversationId);
        return this;
    }

    public AvailabilityCardBuilder WithInitiator(Guid initiatorId)
    {
        _faker.RuleFor(a => a.InitiatorId, initiatorId);
        return this;
    }

    public AvailabilityCardBuilder WithStatus(AvailabilityCardStatus status)
    {
        _faker.RuleFor(a => a.Status, status);
        return this;
    }

    public AvailabilityCardBuilder AsExpired()
    {
        _faker.RuleFor(a => a.ExpiresAt, _ => DateTime.UtcNow.AddHours(-1));
        return this;
    }

    public AvailabilityCardBuilder With<T>(Expression<Func<AvailabilityCard, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public AvailabilityCard Build() => _faker.Generate();
}
