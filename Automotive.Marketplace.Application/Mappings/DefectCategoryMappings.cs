using AutoMapper;
using Automotive.Marketplace.Application.Features.DefectFeatures.GetDefectCategories;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Mappings;

public class DefectCategoryMappings : Profile
{
    public DefectCategoryMappings()
    {
        CreateMap<DefectCategoryTranslation, GetDefectCategoryTranslationResponse>();
        CreateMap<DefectCategory, GetDefectCategoriesResponse>();
    }
}
