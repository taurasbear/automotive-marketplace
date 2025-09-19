using AutoMapper;
using Automotive.Marketplace.Application.Features.EnumFeatures.GetBodyTypes;
using Automotive.Marketplace.Application.Features.EnumFeatures.GetDrivetrainTypes;
using Automotive.Marketplace.Application.Features.EnumFeatures.GetFuelTypes;
using Automotive.Marketplace.Application.Features.EnumFeatures.GetTransmissionTypes;
using Automotive.Marketplace.Domain.Enums;

namespace Automotive.Marketplace.Application.Mappings;

public class EnumMappings : Profile
{
    public EnumMappings()
    {
        CreateMap<Transmission, GetTransmissionTypesResponse>()
            .ForMember(dest => dest.TransmissionType, opt => opt.MapFrom(src => src.ToString()));

        CreateMap<Fuel, GetFuelTypesResponse>()
            .ForMember(dest => dest.FuelType, opt => opt.MapFrom(src => src.ToString()));

        CreateMap<BodyType, GetBodyTypesResponse>()
            .ForMember(dest => dest.BodyType, opt => opt.MapFrom(src => src.ToString()));

        CreateMap<Drivetrain, GetDrivetrainTypesResponse>()
            .ForMember(dest => dest.DrivetrainType, opt => opt.MapFrom(src => src.ToString()));
    }
}