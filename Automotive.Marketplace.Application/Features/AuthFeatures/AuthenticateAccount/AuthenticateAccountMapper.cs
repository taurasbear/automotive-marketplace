namespace Automotive.Marketplace.Application.Features.AuthFeatures.AuthenticateAccount;

using AutoMapper;
using Automotive.Marketplace.Domain.Entities;

public class AuthenticateAccountMapper : Profile
{
    public AuthenticateAccountMapper()
    {
        this.CreateMap<RefreshToken, AuthenticateAccountResponse>()
           .ForMember(dest => dest.FreshExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate))
           .ForMember(dest => dest.FreshRefreshToken, opt => opt.MapFrom(src => src.Token))
           .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountId))
           .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Account.RoleName));
    }
}
