using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class MessageBuilder
{
    private readonly Faker<Message> _faker;

    public MessageBuilder()
    {
        _faker = new Faker<Message>()
            .RuleFor(m => m.Id, f => f.Random.Guid())
            .RuleFor(m => m.Content, f => f.Lorem.Sentence())
            .RuleFor(m => m.SentAt, f => f.Date.Recent().ToUniversalTime())
            .RuleFor(m => m.IsRead, false)
            .RuleFor(m => m.CreatedAt, f => f.Date.Recent().ToUniversalTime())
            .RuleFor(m => m.CreatedBy, f => f.Random.Guid().ToString());
    }

    public MessageBuilder WithConversation(Guid conversationId)
    {
        _faker.RuleFor(m => m.ConversationId, conversationId);
        return this;
    }

    public MessageBuilder WithSender(Guid senderId)
    {
        _faker.RuleFor(m => m.SenderId, senderId);
        return this;
    }

    public MessageBuilder WithIsRead(bool isRead)
    {
        _faker.RuleFor(m => m.IsRead, isRead);
        return this;
    }

    public MessageBuilder With<T>(Expression<Func<Message, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public Message Build() => _faker.Generate();

    public List<Message> Build(int count) => _faker.Generate(count);
}
