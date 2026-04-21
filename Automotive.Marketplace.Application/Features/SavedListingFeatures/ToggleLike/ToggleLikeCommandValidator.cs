using FluentValidation;

namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.ToggleLike;

public sealed class ToggleLikeCommandValidator : AbstractValidator<ToggleLikeCommand>
{
    public ToggleLikeCommandValidator()
    {
        RuleFor(command => command.ListingId)
            .NotEmpty();
        
        RuleFor(command => command.UserId)
            .NotEmpty();
    }
}
