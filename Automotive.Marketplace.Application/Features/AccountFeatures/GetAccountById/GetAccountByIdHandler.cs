namespace Automotive.Marketplace.Application.Features.AccountFeatures.GetAccountById
{
    using AutoMapper;
    using Automotive.Marketplace.Application.Interfaces.Data;
    using System.Threading;
    using System.Threading.Tasks;

    public class GetAccountByIdHandler : BaseHandler<GetAccountByIdRequest, GetAccountByIdResponse>
    {
        public GetAccountByIdHandler(IMapper mapper, IUnitOfWork unitOfWork) : base(mapper, unitOfWork)
        { }

        public override async Task<GetAccountByIdResponse> Handle(GetAccountByIdRequest request, CancellationToken cancellationToken)
        {
            var account = await this.UnitOfWork.AccountRepository.GetAccountByIdAsync(request.accountId, cancellationToken);

            if (account == null)
            {
                return new GetAccountByIdResponse();
            }

            return this.Mapper.Map<GetAccountByIdResponse>(account);
        }
    }
}
