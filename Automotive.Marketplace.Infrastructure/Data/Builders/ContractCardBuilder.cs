using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Automotive.Marketplace.Domain.Enums;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class ContractCardBuilder
{
    private readonly Faker<ContractCard> _faker;

    public ContractCardBuilder()
    {
        _faker = new Faker<ContractCard>()
            .RuleFor(c => c.Id, f => f.Random.Guid())
            .RuleFor(c => c.ConversationId, f => f.Random.Guid())
            .RuleFor(c => c.InitiatorId, f => f.Random.Guid())
            .RuleFor(c => c.Status, ContractCardStatus.Pending)
            .RuleFor(c => c.AcceptedAt, _ => null)
            .RuleFor(c => c.CreatedAt, _ => DateTime.UtcNow)
            .RuleFor(c => c.CreatedBy, f => f.Random.Guid().ToString());
    }

    public ContractCardBuilder WithConversation(Guid conversationId)
    {
        _faker.RuleFor(c => c.ConversationId, conversationId);
        return this;
    }

    public ContractCardBuilder WithInitiator(Guid initiatorId)
    {
        _faker.RuleFor(c => c.InitiatorId, initiatorId);
        return this;
    }

    public ContractCardBuilder WithStatus(ContractCardStatus status)
    {
        _faker.RuleFor(c => c.Status, status);
        return this;
    }

    public ContractCardBuilder With<T>(Expression<Func<ContractCard, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public ContractCard Build() => _faker.Generate();
}
