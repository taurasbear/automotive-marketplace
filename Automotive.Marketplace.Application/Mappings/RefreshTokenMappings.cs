using AutoMapper;
using Automotive.Marketplace.Application.Features.AuthFeatures.LoginUser;
using Automotive.Marketplace.Application.Features.AuthFeatures.RefreshToken;
using Automotive.Marketplace.Application.Features.AuthFeatures.RegisterUser;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Mappings;

public class RefreshTokenMappings : Profile
{
    public RefreshTokenMappings()
    {
        CreateMap<RefreshToken, LoginUserResponse>()
           .ForMember(dest => dest.FreshExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate))
           .ForMember(dest => dest.FreshRefreshToken, opt => opt.MapFrom(src => src.Token))
           .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));

        CreateMap<RefreshToken, RefreshTokenResponse>()
         .ForMember(dest => dest.FreshRefreshToken, opt => opt.MapFrom(src => src.Token))
         .ForMember(dest => dest.FreshExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate))
         .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));

        CreateMap<RefreshToken, RegisterUserResponse>()
           .ForMember(dest => dest.RefreshToken, opt => opt.MapFrom(src => src.Token))
           .ForMember(dest => dest.RefreshTokenExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate))
           .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));
    }
}