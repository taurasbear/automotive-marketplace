using FluentValidation;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.DeleteListingNote;

public sealed class DeleteListingNoteCommandValidator : AbstractValidator<DeleteListingNoteCommand>
{
    public DeleteListingNoteCommandValidator()
    {
        RuleFor(command => command.ListingId)
            .NotEmpty();
    }
}
