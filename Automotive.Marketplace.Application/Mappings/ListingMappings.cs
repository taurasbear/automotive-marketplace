using AutoMapper;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Mappings;

public class ListingMapping : Profile
{
    public ListingMapping()
    {
        CreateMap<Listing, GetAllListingsResponse>()
            .ForMember(dest => dest.Mileage, opt => opt.MapFrom(src => src.CarDetails.Mileage))
            .ForMember(dest => dest.Power, opt => opt.MapFrom(src => src.CarDetails.Power))
            .ForMember(dest => dest.EngineSize, opt => opt.MapFrom(src => src.CarDetails.EngineSize))
            .ForMember(dest => dest.Used, opt => opt.MapFrom(src => src.CarDetails.Used))
            .ForMember(dest => dest.FuelType, opt => opt.MapFrom(src => src.CarDetails.Car.Fuel.ToString()))
            .ForMember(dest => dest.Transmission, opt => opt.MapFrom(src => src.CarDetails.Car.Transmission.ToString()))
            .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.CarDetails.Car.Year.Year.ToString()))
            .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.CarDetails.Car.Model.Name))
            .ForMember(dest => dest.Make, opt => opt.MapFrom(src => src.CarDetails.Car.Model.Make.Name));
    }
}