using FluentValidation;

namespace Automotive.Marketplace.Application.Features.AuthFeatures.RegisterUser;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(credentials => credentials.Email)
            .EmailAddress();
    }
}
