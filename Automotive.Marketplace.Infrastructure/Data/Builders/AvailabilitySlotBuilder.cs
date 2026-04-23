using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class AvailabilitySlotBuilder
{
    private readonly Faker<AvailabilitySlot> _faker;

    public AvailabilitySlotBuilder()
    {
        _faker = new Faker<AvailabilitySlot>()
            .RuleFor(s => s.Id, f => f.Random.Guid())
            .RuleFor(s => s.AvailabilityCardId, f => f.Random.Guid())
            .RuleFor(s => s.StartTime, _ => DateTime.UtcNow.AddDays(2))
            .RuleFor(s => s.EndTime, _ => DateTime.UtcNow.AddDays(2).AddHours(1))
            .RuleFor(s => s.CreatedAt, _ => DateTime.UtcNow)
            .RuleFor(s => s.CreatedBy, f => f.Random.Guid().ToString());
    }

    public AvailabilitySlotBuilder WithCard(Guid cardId)
    {
        _faker.RuleFor(s => s.AvailabilityCardId, cardId);
        return this;
    }

    public AvailabilitySlotBuilder WithTimes(DateTime start, DateTime end)
    {
        _faker.RuleFor(s => s.StartTime, start);
        _faker.RuleFor(s => s.EndTime, end);
        return this;
    }

    public AvailabilitySlotBuilder With<T>(Expression<Func<AvailabilitySlot, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public AvailabilitySlot Build() => _faker.Generate();
}
