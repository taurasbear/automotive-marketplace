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
        CreateMap<Listing, CreateListingResponse>();

        CreateMap<Listing, GetAllListingsResponse>()
            .ForMember(dest => dest.VariantId, opt => opt.MapFrom(src => src.VariantId))
            .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Variant.Year))
            .ForMember(dest => dest.MakeName, opt => opt.MapFrom(src => src.Variant.Model.Make.Name))
            .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.Variant.Model.Name))
            .ForMember(dest => dest.FuelName, opt => opt.MapFrom(src => src.Variant.Fuel.Name))
            .ForMember(dest => dest.TransmissionName, opt => opt.MapFrom(src => src.Variant.Transmission.Name))
            .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => src.Seller.Username))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ThumbnailUrl, opt => opt.Ignore());

        CreateMap<UpdateListingCommand, Listing>()
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Colour, opt => opt.MapFrom(src => src.Colour))
            .ForMember(dest => dest.Vin, opt => opt.MapFrom(src => src.Vin))
            .ForMember(dest => dest.Mileage, opt => opt.MapFrom(src => src.Mileage))
            .ForMember(dest => dest.IsSteeringWheelRight, opt => opt.MapFrom(src => src.IsSteeringWheelRight))
            .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City))
            .ForMember(dest => dest.IsUsed, opt => opt.MapFrom(src => src.IsUsed));

        CreateMap<Listing, GetListingByIdResponse>()
            .ForMember(dest => dest.VariantId, opt => opt.MapFrom(src => src.VariantId))
            .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Variant.Year))
            .ForMember(dest => dest.MakeName, opt => opt.MapFrom(src => src.Variant.Model.Make.Name))
            .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.Variant.Model.Name))
            .ForMember(dest => dest.FuelName, opt => opt.MapFrom(src => src.Variant.Fuel.Name))
            .ForMember(dest => dest.TransmissionName, opt => opt.MapFrom(src => src.Variant.Transmission.Name))
            .ForMember(dest => dest.BodyTypeName, opt => opt.MapFrom(src => src.Variant.BodyType.Name))
            .ForMember(dest => dest.DrivetrainName, opt => opt.MapFrom(src => src.Drivetrain.Name))
            .ForMember(dest => dest.DoorCount, opt => opt.MapFrom(src => src.Variant.DoorCount))
            .ForMember(dest => dest.PowerKw, opt => opt.MapFrom(src => src.Variant.PowerKw))
            .ForMember(dest => dest.EngineSizeMl, opt => opt.MapFrom(src => src.Variant.EngineSizeMl))
            .ForMember(dest => dest.SellerName, opt => opt.MapFrom(src => src.Seller.Username))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.ImageUrls, opt => opt.Ignore());
    }
}