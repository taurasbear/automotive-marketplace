namespace Automotive.Marketplace.Application.Features.AccountFeatures.GetAccountById
{
    using AutoMapper;
    using Automotive.Marketplace.Domain.Entities;

    public class GetAccountByIdMapper : Profile
    {
        public GetAccountByIdMapper()
        {
            this.CreateMap<Account, GetAccountByIdResponse>()
                .ForMember(dest => dest.Account, opt => opt.MapFrom(src => src));
        }
    }
}
