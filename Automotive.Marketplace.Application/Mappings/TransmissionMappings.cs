using AutoMapper;
using Automotive.Marketplace.Application.Features.TransmissionFeatures.GetAllTransmissions;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Mappings;

public class TransmissionMappings : Profile
{
    public TransmissionMappings()
    {
        CreateMap<TransmissionTranslation, GetAllTransmissionTranslationResponse>();

        CreateMap<Transmission, GetAllTransmissionsResponse>();
    }
}
