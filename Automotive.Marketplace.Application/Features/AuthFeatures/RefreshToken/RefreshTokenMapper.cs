namespace Automotive.Marketplace.Application.Features.AuthFeatures.RefreshToken;

using AutoMapper;
using Automotive.Marketplace.Domain.Entities;

public class RefreshTokenMapper : Profile
{
    public RefreshTokenMapper()
    {
        this.CreateMap<RefreshToken, RefreshTokenResponse>()
            .ForMember(dest => dest.FreshRefreshToken, opt => opt.MapFrom(src => src.Token))
            .ForMember(dest => dest.FreshExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate));
    }
}