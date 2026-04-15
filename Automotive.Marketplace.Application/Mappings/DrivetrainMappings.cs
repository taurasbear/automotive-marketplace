using AutoMapper;
using Automotive.Marketplace.Application.Features.DrivetrainFeatures.GetAllDrivetrains;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Mappings;

public class DrivetrainMappings : Profile
{
    public DrivetrainMappings()
    {
        CreateMap<DrivetrainTranslation, GetAllDrivetrainTranslationResponse>();

        CreateMap<Drivetrain, GetAllDrivetrainsResponse>();
    }
}
