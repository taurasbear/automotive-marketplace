using AutoMapper;
using Automotive.Marketplace.Application.Features.AuthFeatures.AuthenticateAccount;
using Automotive.Marketplace.Application.Features.AuthFeatures.RefreshToken;
using Automotive.Marketplace.Application.Features.AuthFeatures.RegisterAccount;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Mappings;

public class RefreshTokenMappings : Profile
{
    public RefreshTokenMappings()
    {
        CreateMap<RefreshToken, AuthenticateAccountResponse>()
           .ForMember(dest => dest.FreshExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate))
           .ForMember(dest => dest.FreshRefreshToken, opt => opt.MapFrom(src => src.Token))
           .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountId))
           .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.User.RoleName));

        CreateMap<RefreshToken, RefreshTokenResponse>()
         .ForMember(dest => dest.FreshRefreshToken, opt => opt.MapFrom(src => src.Token))
         .ForMember(dest => dest.FreshExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate));

        CreateMap<RefreshToken, RegisterAccountResponse>()
           .ForMember(dest => dest.RefreshToken, opt => opt.MapFrom(src => src.Token))
           .ForMember(dest => dest.RefreshTokenExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate))
           .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountId))
           .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.User.RoleName));
    }
}