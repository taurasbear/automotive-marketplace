namespace Automotive.Marketplace.Application.Features.AuthFeatures.RegisterAccount;

using FluentValidation;

public class RegisterAccountCommandValidator : AbstractValidator<RegisterAccountCommand>
{
    public RegisterAccountCommandValidator()
    {
        RuleFor(credentials => credentials.Email)
            .EmailAddress();
    }
}
