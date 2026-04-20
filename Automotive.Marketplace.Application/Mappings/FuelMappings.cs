using AutoMapper;
using Automotive.Marketplace.Application.Features.FuelFeatures.GetAllFuels;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Mappings;

public class FuelMappings : Profile
{
    public FuelMappings()
    {
        CreateMap<FuelTranslation, GetAllFuelTranslationResponse>();

        CreateMap<Fuel, GetAllFuelsResponse>();
    }
}
