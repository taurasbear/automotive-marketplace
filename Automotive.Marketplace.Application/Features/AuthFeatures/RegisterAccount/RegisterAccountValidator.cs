namespace Automotive.Marketplace.Application.Features.AuthFeatures.RegisterAccount;

using FluentValidation;

public class RegisterAccountValidator : AbstractValidator<RegisterAccountRequest>
{
    public RegisterAccountValidator()
    {
        this.RuleFor(credentials => credentials.email)
            .EmailAddress();
    }
}
