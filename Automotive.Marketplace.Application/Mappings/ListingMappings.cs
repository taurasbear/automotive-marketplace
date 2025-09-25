using AutoMapper;
using Automotive.Marketplace.Application.Features.ListingFeatures.CreateListing;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingById;
using Automotive.Marketplace.Application.Features.ListingFeatures.UpdateListing;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Mappings;

public class ListingMapping : Profile
{
    public ListingMapping()
    {
        CreateMap<Listing, GetAllListingsResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Mileage, opt => opt.MapFrom(src => src.Mileage))
            .ForMember(dest => dest.Power, opt => opt.MapFrom(src => src.Power))
            .ForMember(dest => dest.EngineSize, opt => opt.MapFrom(src => src.EngineSize))
            .ForMember(dest => dest.IsUsed, opt => opt.MapFrom(src => src.IsUsed))
            .ForMember(dest => dest.FuelType, opt => opt.MapFrom(src => src.Car.Fuel.ToString()))
            .ForMember(dest => dest.Transmission, opt => opt.MapFrom(src => src.Car.Transmission.ToString()))
            .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Car.Year.Year.ToString()))
            .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Car.Model.Name))
            .ForMember(dest => dest.Make, opt => opt.MapFrom(src => src.Car.Model.Make.Name))
            .ForMember(dest => dest.Images, opt => opt.Ignore());

        CreateMap<CreateListingCommand, Listing>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Colour, opt => opt.MapFrom(src => src.Colour))
            .ForMember(dest => dest.Vin, opt => opt.MapFrom(src => src.Vin))
            .ForMember(dest => dest.Power, opt => opt.MapFrom(src => src.Power))
            .ForMember(dest => dest.EngineSize, opt => opt.MapFrom(src => src.EngineSize))
            .ForMember(dest => dest.Mileage, opt => opt.MapFrom(src => src.Mileage))
            .ForMember(dest => dest.IsSteeringWheelRight, opt => opt.MapFrom(src => src.IsSteeringWheelRight))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.IsUsed, opt => opt.MapFrom(src => src.IsUsed))
            .ForMember(dest => dest.SellerId, opt => opt.MapFrom(src => src.UserId))
            .ForMember(dest => dest.Images, opt => opt.Ignore());

        CreateMap<UpdateListingCommand, Listing>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Colour, opt => opt.MapFrom(src => src.Colour))
            .ForMember(dest => dest.Vin, opt => opt.MapFrom(src => src.Vin))
            .ForMember(dest => dest.Power, opt => opt.MapFrom(src => src.Power))
            .ForMember(dest => dest.EngineSize, opt => opt.MapFrom(src => src.EngineSize))
            .ForMember(dest => dest.Mileage, opt => opt.MapFrom(src => src.Mileage))
            .ForMember(dest => dest.IsSteeringWheelRight, opt => opt.MapFrom(src => src.IsSteeringWheelRight))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.IsUsed, opt => opt.MapFrom(src => src.IsUsed))
            .ForMember(dest => dest.Images, opt => opt.Ignore());

        CreateMap<Listing, GetListingByIdResponse>()
            .ForMember(dest => dest.Make, opt => opt.MapFrom(src => src.Car.Model.Make.Name))
            .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Car.Model.Name))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Colour, opt => opt.MapFrom(src => src.Colour))
            .ForMember(dest => dest.Vin, opt => opt.MapFrom(src => src.Vin))
            .ForMember(dest => dest.Power, opt => opt.MapFrom(src => src.Power))
            .ForMember(dest => dest.EngineSize, opt => opt.MapFrom(src => src.EngineSize))
            .ForMember(dest => dest.Mileage, opt => opt.MapFrom(src => src.Mileage))
            .ForMember(dest => dest.IsSteeringWheelRight, opt => opt.MapFrom(src => src.IsSteeringWheelRight))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.IsUsed, opt => opt.MapFrom(src => src.IsUsed))
            .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Car.Year.Year))
            .ForMember(dest => dest.Transmission, opt => opt.MapFrom(src => src.Car.Transmission.ToString()))
            .ForMember(dest => dest.Fuel, opt => opt.MapFrom(src => src.Car.Fuel.ToString()))
            .ForMember(dest => dest.DoorCount, opt => opt.MapFrom(src => src.Car.DoorCount))
            .ForMember(dest => dest.BodyType, opt => opt.MapFrom(src => src.Car.BodyType.ToString()))
            .ForMember(dest => dest.Drivetrain, opt => opt.MapFrom(src => src.Car.Drivetrain.ToString()))
            .ForMember(dest => dest.Seller, opt => opt.MapFrom(src => src.Seller.Username))
            .ForMember(dest => dest.Images, opt => opt.Ignore());
    }
}