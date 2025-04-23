namespace Automotive.Marketplace.Application.Features.AuthFeatures.RegisterAccount
{
    using AutoMapper;
    using Automotive.Marketplace.Domain.Entities;

    public class RegisterAccountMapper : Profile
    {
        public RegisterAccountMapper()
        {
            this.CreateMap<Account, RegisterAccountResponse>()
                .ForMember(dest => dest.Account, opt => opt.MapFrom(src => src));
        }
    }
}
