using AutoMapper;
using Automotive.Marketplace.Application.Features.MakeFeatures.CreateMake;
using Automotive.Marketplace.Application.Features.MakeFeatures.GetAllMakes;
using Automotive.Marketplace.Application.Features.MakeFeatures.GetMakeById;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Mappings;

public class MakeMappings : Profile
{
    public MakeMappings()
    {
        CreateMap<Make, GetAllMakesResponse>();
        CreateMap<Make, GetMakeByIdResponse>();
        CreateMap<CreateMakeCommand, Make>();
    }
}