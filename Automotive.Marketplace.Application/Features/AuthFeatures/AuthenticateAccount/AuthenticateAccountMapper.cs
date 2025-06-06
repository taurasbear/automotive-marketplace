namespace Automotive.Marketplace.Application.Features.AuthFeatures.AuthenticateAccount;

using AutoMapper;
using Automotive.Marketplace.Domain.Entities;

public class AuthenticateAccountMapper : Profile
{
    public AuthenticateAccountMapper()
    {
        //this.CreateMap<Account, AuthenticateAccountResponse>()
        //    .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.Id))
        //    .ForMember(dest => dest.UserRole, opt => opt.MapFrom(src => src.RoleName);
    }
}
