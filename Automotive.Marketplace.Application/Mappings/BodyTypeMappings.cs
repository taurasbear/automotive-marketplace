using AutoMapper;
using Automotive.Marketplace.Application.Features.BodyTypeFeatures.GetAllBodyTypes;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Mappings;

public class BodyTypeMappings : Profile
{
    public BodyTypeMappings()
    {
        CreateMap<BodyTypeTranslation, GetAllBodyTypeTranslationResponse>();

        CreateMap<BodyType, GetAllBodyTypesResponse>();
    }
}
