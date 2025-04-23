namespace Automotive.Marketplace.Application.Features.AccountFeatures.GetAccountById
{
    using MediatR;

    public sealed record GetAccountByIdRequest(Guid accountId) : IRequest<GetAccountByIdResponse>;
}
