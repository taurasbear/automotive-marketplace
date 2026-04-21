using System.Linq.Expressions;
using Automotive.Marketplace.Domain.Entities;
using Bogus;

namespace Automotive.Marketplace.Infrastructure.Data.Builders;

public class UserListingNoteBuilder
{
    private readonly Faker<UserListingNote> _faker;

    public UserListingNoteBuilder()
    {
        _faker = new Faker<UserListingNote>()
            .RuleFor(note => note.Id, f => f.Random.Guid())
            .RuleFor(note => note.UserId, f => f.Random.Guid())
            .RuleFor(note => note.ListingId, f => f.Random.Guid())
            .RuleFor(note => note.Content, f => f.Lorem.Sentence())
            .RuleFor(note => note.CreatedAt, f => f.Date.Past(1).ToUniversalTime())
            .RuleFor(note => note.CreatedBy, f => f.Random.Guid().ToString());
    }

    public UserListingNoteBuilder WithUser(Guid userId)
    {
        _faker.RuleFor(note => note.UserId, userId);
        return this;
    }

    public UserListingNoteBuilder WithListing(Guid listingId)
    {
        _faker.RuleFor(note => note.ListingId, listingId);
        return this;
    }

    public UserListingNoteBuilder WithContent(string content)
    {
        _faker.RuleFor(note => note.Content, content);
        return this;
    }

    public UserListingNoteBuilder With<T>(Expression<Func<UserListingNote, T>> property, T value)
    {
        _faker.RuleFor(property, value);
        return this;
    }

    public UserListingNote Build() => _faker.Generate();
}
