using AutoMapper;
using Automotive.Marketplace.Application.Features.ModelFeatures.CreateModel;
using Automotive.Marketplace.Application.Features.ModelFeatures.GetAllModels;
using Automotive.Marketplace.Application.Features.ModelFeatures.GetModelById;
using Automotive.Marketplace.Application.Features.ModelFeatures.GetModelsByMakeId;
using Automotive.Marketplace.Application.Features.ModelFeatures.UpdateModel;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Mappings;

public class ModelMappings : Profile
{
    public ModelMappings()
    {
        CreateMap<Model, GetModelsByMakeIdResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

        CreateMap<Model, GetModelByIdResponse>();

        CreateMap<Model, GetAllModelsResponse>();

        CreateMap<CreateModelCommand, Model>();

        CreateMap<UpdateModelCommand, Model>();
    }
}