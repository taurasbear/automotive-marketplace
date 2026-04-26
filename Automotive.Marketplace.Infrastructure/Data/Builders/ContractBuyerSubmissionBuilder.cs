using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class ContractBuyerSubmissionBuilder
{
    private readonly Faker<ContractBuyerSubmission> _faker;

    public ContractBuyerSubmissionBuilder()
    {
        _faker = new Faker<ContractBuyerSubmission>()
            .RuleFor(b => b.Id, f => f.Random.Guid())
            .RuleFor(b => b.ContractCardId, f => f.Random.Guid())
            .RuleFor(b => b.PersonalIdCode, f => f.Random.String2(11, "0123456789"))
            .RuleFor(b => b.FullName, f => f.Name.FullName())
            .RuleFor(b => b.Phone, f => f.Phone.PhoneNumber())
            .RuleFor(b => b.Email, f => f.Internet.Email())
            .RuleFor(b => b.Address, f => f.Address.FullAddress())
            .RuleFor(b => b.SubmittedAt, _ => DateTime.UtcNow)
            .RuleFor(b => b.CreatedAt, _ => DateTime.UtcNow)
            .RuleFor(b => b.CreatedBy, f => f.Random.Guid().ToString());
    }

    public ContractBuyerSubmissionBuilder WithContractCard(Guid contractCardId)
    {
        _faker.RuleFor(b => b.ContractCardId, contractCardId);
        return this;
    }

    public ContractBuyerSubmissionBuilder With<T>(Expression<Func<ContractBuyerSubmission, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public ContractBuyerSubmission Build() => _faker.Generate();
}
