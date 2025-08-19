using FluentValidation;

namespace Automotive.Marketplace.Application.Features.AuthFeatures.RegisterAccount;

public class RegisterAccountCommandValidator : AbstractValidator<RegisterAccountCommand>
{
    public RegisterAccountCommandValidator()
    {
        RuleFor(credentials => credentials.Email)
            .EmailAddress();
    }
}
