namespace Automotive.Marketplace.Application.Features.AuthFeatures.GetRefreshTokenByToken
{
    using AutoMapper;
    using Automotive.Marketplace.Application.Interfaces.Data;

    public class GetRefreshTokenByTokenHandler : BaseHandler<GetRefreshTokenByTokenRequest, GetRefreshTokenByTokenResponse>
    {
        public GetRefreshTokenByTokenHandler(IMapper mapper, IUnitOfWork unitOfWork) : base(mapper, unitOfWork)
        {
        }

        public override async Task<GetRefreshTokenByTokenResponse> Handle(GetRefreshTokenByTokenRequest request, CancellationToken cancellationToken)
        {
            var refreshToken = await this.UnitOfWork.RefreshTokenRepository.GetRefreshTokenByTokenAsync(request.token, cancellationToken);

            if (refreshToken == null)
            {
                return new GetRefreshTokenByTokenResponse();
            }

            return this.Mapper.Map<GetRefreshTokenByTokenResponse>(refreshToken);
        }
    }
}
