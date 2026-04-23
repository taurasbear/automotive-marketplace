using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class MeetingBuilder
{
    private readonly Faker<Meeting> _faker;

    public MeetingBuilder()
    {
        _faker = new Faker<Meeting>()
            .RuleFor(m => m.Id, f => f.Random.Guid())
            .RuleFor(m => m.ConversationId, f => f.Random.Guid())
            .RuleFor(m => m.InitiatorId, f => f.Random.Guid())
            .RuleFor(m => m.ProposedAt, _ => DateTime.UtcNow.AddDays(3))
            .RuleFor(m => m.DurationMinutes, 60)
            .RuleFor(m => m.LocationText, f => f.Address.FullAddress())
            .RuleFor(m => m.Status, MeetingStatus.Pending)
            .RuleFor(m => m.ExpiresAt, _ => DateTime.UtcNow.AddHours(48))
            .RuleFor(m => m.CreatedAt, _ => DateTime.UtcNow)
            .RuleFor(m => m.CreatedBy, f => f.Random.Guid().ToString());
    }

    public MeetingBuilder WithConversation(Guid conversationId)
    {
        _faker.RuleFor(m => m.ConversationId, conversationId);
        return this;
    }

    public MeetingBuilder WithInitiator(Guid initiatorId)
    {
        _faker.RuleFor(m => m.InitiatorId, initiatorId);
        return this;
    }

    public MeetingBuilder WithProposedAt(DateTime proposedAt)
    {
        _faker.RuleFor(m => m.ProposedAt, proposedAt);
        return this;
    }

    public MeetingBuilder WithStatus(MeetingStatus status)
    {
        _faker.RuleFor(m => m.Status, status);
        return this;
    }

    public MeetingBuilder WithParent(Guid parentMeetingId)
    {
        _faker.RuleFor(m => m.ParentMeetingId, parentMeetingId);
        return this;
    }

    public MeetingBuilder AsExpired()
    {
        _faker.RuleFor(m => m.ExpiresAt, _ => DateTime.UtcNow.AddHours(-1));
        return this;
    }

    public MeetingBuilder With<T>(Expression<Func<Meeting, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public Meeting Build() => _faker.Generate();
}
