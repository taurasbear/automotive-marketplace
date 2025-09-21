using AutoMapper;
using Automotive.Marketplace.Application.Features.MakeFeatures.GetAllMakes;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Mappings;

public class MakeMappings : Profile
{
    public MakeMappings()
    {
        CreateMap<Make, GetAllMakesResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));
    }
}