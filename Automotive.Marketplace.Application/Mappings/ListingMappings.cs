using AutoMapper;
using Automotive.Marketplace.Application.Features.ListingFeatures.CreateListing;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetAllListings;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetListingById;
using Automotive.Marketplace.Application.Features.ListingFeatures.GetMyListings;
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
            .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year))
            .ForMember(dest => dest.MakeName, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.Model?.Make?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.ModelName, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.Model?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.FuelName, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.Fuel?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.TransmissionName, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.Transmission?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.PowerKw, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.PowerKw : 0))
            .ForMember(dest => dest.EngineSizeMl, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.EngineSizeMl : 0))
            .ForMember(dest => dest.SellerName, opt => opt.MapFrom((src, dest) => src.Seller != null ? src.Seller.Username ?? string.Empty : string.Empty))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Thumbnail, opt => opt.Ignore())
            .ForMember(dest => dest.IsLiked, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.ImageCount, opt => opt.Ignore())
            .ForMember(dest => dest.DefectCount, opt => opt.Ignore());

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
            .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.Year))
            .ForMember(dest => dest.MakeName, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.Model?.Make?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.ModelName, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.Model?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.FuelName, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.Fuel?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.TransmissionName, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.Transmission?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.BodyTypeName, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.BodyType?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.DrivetrainName, opt => opt.MapFrom((src, dest) => src.Drivetrain != null ? src.Drivetrain.Name : string.Empty))
            .ForMember(dest => dest.DoorCount, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.DoorCount : 0))
            .ForMember(dest => dest.PowerKw, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.PowerKw : 0))
            .ForMember(dest => dest.EngineSizeMl, opt => opt.MapFrom((src, dest) => src.Variant != null ? src.Variant.EngineSizeMl : 0))
            .ForMember(dest => dest.SellerName, opt => opt.MapFrom((src, dest) => src.Seller != null ? src.Seller.Username ?? string.Empty : string.Empty))
            .ForMember(dest => dest.Colour, opt => opt.MapFrom(src => src.Colour))
            .ForMember(dest => dest.Vin, opt => opt.MapFrom(src => src.Vin))
            .ForMember(dest => dest.IsSteeringWheelRight, opt => opt.MapFrom(src => src.IsSteeringWheelRight))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description ?? string.Empty))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.Defects, opt => opt.Ignore());

        CreateMap<Listing, GetMyListingsResponse>()
            .ForMember(dest => dest.MakeName, opt => opt.MapFrom((src, _) => src.Variant != null ? src.Variant.Model?.Make?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.ModelName, opt => opt.MapFrom((src, _) => src.Variant != null ? src.Variant.Model?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.FuelName, opt => opt.MapFrom((src, _) => src.Variant != null ? src.Variant.Fuel?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.TransmissionName, opt => opt.MapFrom((src, _) => src.Variant != null ? src.Variant.Transmission?.Name ?? string.Empty : string.Empty))
            .ForMember(dest => dest.PowerKw, opt => opt.MapFrom((src, _) => src.Variant != null ? src.Variant.PowerKw : 0))
            .ForMember(dest => dest.EngineSizeMl, opt => opt.MapFrom((src, _) => src.Variant != null ? src.Variant.EngineSizeMl : 0))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Thumbnail, opt => opt.Ignore())
            .ForMember(dest => dest.ImageCount, opt => opt.Ignore())
            .ForMember(dest => dest.DefectCount, opt => opt.Ignore())
            .ForMember(dest => dest.Images, opt => opt.Ignore())
            .ForMember(dest => dest.LikeCount, opt => opt.Ignore())
            .ForMember(dest => dest.ConversationCount, opt => opt.Ignore());
    }
}