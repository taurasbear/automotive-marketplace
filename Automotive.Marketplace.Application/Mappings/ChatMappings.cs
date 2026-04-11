using AutoMapper;
using Automotive.Marketplace.Application.Features.ChatFeatures.GetMessages;
using Automotive.Marketplace.Domain.Entities;

namespace Automotive.Marketplace.Application.Mappings;

public class ChatMappings : Profile
{
    public ChatMappings()
    {
        CreateMap<Message, GetMessagesResponse.MessageDto>()
            .ForMember(dest => dest.SenderUsername, opt => opt.MapFrom(src => src.Sender.Username));
    }
}
