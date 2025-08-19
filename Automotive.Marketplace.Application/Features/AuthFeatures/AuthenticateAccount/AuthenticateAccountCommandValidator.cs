using FluentValidation;

namespace Automotive.Marketplace.Application.Features.AuthFeatures.AuthenticateAccount;

public sealed class AuthenticateAccountCommandValidator : AbstractValidator<AuthenticateAccountCommand>
{
    public AuthenticateAccountCommandValidator()
    {
        RuleFor(credentials => credentials.Email)
            .EmailAddress();
    }
}
