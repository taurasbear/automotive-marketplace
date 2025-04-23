namespace Automotive.Marketplace.Application.Features.AuthFeatures.AuthenticateAccount
{
    using Automotive.Marketplace.Domain.Entities;

    public sealed record AuthenticateAccountResponse
    {
        public Account? Account { get; set; }
    }
}
