using FluentValidation;

namespace Automotive.Marketplace.Application.Features.AuthFeatures.LoginUser;

public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(credentials => credentials.Email)
            .EmailAddress();
    }
}
