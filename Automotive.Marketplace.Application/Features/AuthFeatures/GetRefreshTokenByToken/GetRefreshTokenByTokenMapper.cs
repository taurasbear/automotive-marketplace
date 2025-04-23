namespace Automotive.Marketplace.Application.Features.AuthFeatures.GetRefreshTokenByToken
{
    using AutoMapper;
    using Automotive.Marketplace.Domain.Entities;

    public class GetRefreshTokenByTokenMapper : Profile
    {
        public GetRefreshTokenByTokenMapper()
        {
            this.CreateMap<RefreshToken, GetRefreshTokenByTokenResponse>()
                .ForMember(dest => dest.RefreshToken, opt => opt.MapFrom(src => src));
        }
    }
}
