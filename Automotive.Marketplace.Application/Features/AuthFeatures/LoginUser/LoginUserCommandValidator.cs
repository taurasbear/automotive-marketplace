using Automotive.Marketplace.Application.Features.AuthFeatures.LoginUser;
using FluentValidation;

namespace Automotive.Marketplace.Application.Features.AuthFeatures.AuthenticateAccount;

public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(credentials => credentials.Email)
            .EmailAddress();
    }
}
