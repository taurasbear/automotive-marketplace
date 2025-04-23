namespace Automotive.Marketplace.Application.Features.AccountFeatures.GetAccountById
{
    using Automotive.Marketplace.Domain.Entities;

    public sealed record GetAccountByIdResponse
    {
        public Account? Account { get; set; }
    }
}
