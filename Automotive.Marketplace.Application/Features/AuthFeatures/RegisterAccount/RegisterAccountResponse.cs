namespace Automotive.Marketplace.Application.Features.AuthFeatures.RegisterAccount
{
    using Automotive.Marketplace.Domain.Entities;

    public sealed record RegisterAccountResponse
    {
        public Account Account { get; set; } = null!;
    }
}
