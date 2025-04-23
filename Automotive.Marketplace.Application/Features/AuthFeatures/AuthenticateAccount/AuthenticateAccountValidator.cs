namespace Automotive.Marketplace.Application.Features.AuthFeatures.AuthenticateAccount
{
    using FluentValidation;

    public sealed class AuthenticateAccountValidator : AbstractValidator<AuthenticateAccountRequest>
    {
        public AuthenticateAccountValidator()
        {
            this.RuleFor(credentials => credentials.email)
                .EmailAddress();
        }
    }
}
