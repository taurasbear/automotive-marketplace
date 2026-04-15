using Automotive.Marketplace.Application.Features.VariantFeatures.CreateVariant;
using Automotive.Marketplace.Application.Features.VariantFeatures.UpdateVariant;
using Automotive.Marketplace.Application.Features.VariantFeatures.GetVariantsByModel;
using Automotive.Marketplace.Domain.Entities;
using AutoMapper;

namespace Automotive.Marketplace.Application.Mappings;

public class VariantMappings : Profile
{
    public VariantMappings()
    {
        CreateMap<CreateVariantCommand, Variant>();
        CreateMap<Variant, CreateVariantResponse>();
        CreateMap<Variant, UpdateVariantResponse>();
        CreateMap<Variant, GetVariantsByModelResponse>()
            .ForMember(dest => dest.FuelName, opt => opt.MapFrom(src => src.Fuel != null ? src.Fuel.Name : string.Empty))
            .ForMember(dest => dest.TransmissionName, opt => opt.MapFrom(src => src.Transmission != null ? src.Transmission.Name : string.Empty))
            .ForMember(dest => dest.BodyTypeName, opt => opt.MapFrom(src => src.BodyType != null ? src.BodyType.Name : string.Empty));
    }
}
