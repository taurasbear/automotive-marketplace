using AutoMapper;
using Automotive.Marketplace.Application.Features.ListingFeatures.CreateListing;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Mappings;

public class CarMappings : Profile
{
    public CarMappings()
    {
        CreateMap<CreateListingCommand, Car>()
            .ForMember(dest => dest.BodyType, opt => opt.MapFrom(src => src.BodyType))
            .ForMember(dest => dest.DoorCount, opt => opt.MapFrom(src => src.DoorCount))
            .ForMember(dest => dest.Drivetrain, opt => opt.MapFrom(src => src.Drivetrain))
            .ForMember(dest => dest.Year, opt => opt.MapFrom(src => new DateTime(src.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc)))
            .ForMember(dest => dest.Fuel, opt => opt.MapFrom(src => src.Fuel))
            .ForMember(dest => dest.Transmission, opt => opt.MapFrom(src => src.Transmission))
            .ForMember(dest => dest.ModelId, opt => opt.MapFrom(src => src.ModelId));
    }
}