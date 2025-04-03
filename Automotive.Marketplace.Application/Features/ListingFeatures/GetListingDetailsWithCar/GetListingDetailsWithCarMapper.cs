namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingDetailsWithCar
{
    using AutoMapper;
    using Automotive.Marketplace.Domain.Entities;

    public class GetListingDetailsWithCarMapper : Profile
    {
        public GetListingDetailsWithCarMapper()
        {
            this.CreateMap<Listing, GetListingDetailsWithCarResponse.GetListingWithCarResponse>()
                .ForMember(dest => dest.Mileage, opt => opt.MapFrom(src => src.CarDetails.Mileage))
                .ForMember(dest => dest.Power, opt => opt.MapFrom(src => src.CarDetails.Power))
                .ForMember(dest => dest.EngineSize, opt => opt.MapFrom(src => src.CarDetails.EngineSize))
                .ForMember(dest => dest.Year, opt => opt.MapFrom(src => src.CarDetails.Car.Year))
                .ForMember(dest => dest.ModelName, opt => opt.MapFrom(src => src.CarDetails.Car.Model.Name));

            this.CreateMap<IList<Listing>, GetListingDetailsWithCarResponse>()
                .ForMember(dest => dest.ListingDetailsWithCar, opt => opt.MapFrom(src => src));
        }
    }
}
