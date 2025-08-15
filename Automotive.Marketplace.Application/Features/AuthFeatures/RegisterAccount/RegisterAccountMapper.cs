namespace Automotive.Marketplace.Application.Features.AuthFeatures.RegisterAccount;

using AutoMapper;
using Automotive.Marketplace.Domain.Entities;

public class RegisterAccountMapper : Profile
{
    public RegisterAccountMapper()
    {
        this.CreateMap<RefreshToken, RegisterAccountResponse>()
            .ForMember(dest => dest.RefreshToken, opt => opt.MapFrom(src => src.Token))
            .ForMember(dest => dest.RefreshTokenExpiryDate, opt => opt.MapFrom(src => src.ExpiryDate))
            .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountId))
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Account.RoleName));
    }
}