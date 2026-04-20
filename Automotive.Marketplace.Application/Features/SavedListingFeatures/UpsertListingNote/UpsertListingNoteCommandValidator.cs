using FluentValidation;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.UpsertListingNote;

public sealed class UpsertListingNoteCommandValidator : AbstractValidator<UpsertListingNoteCommand>
{
    public UpsertListingNoteCommandValidator()
    {
        RuleFor(command => command.Content)
            .NotEmpty()
            .MaximumLength(2000);

        RuleFor(command => command.ListingId)
            .NotEmpty();
    }
}
