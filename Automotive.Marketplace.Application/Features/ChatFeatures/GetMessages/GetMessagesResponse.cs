namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetMessages;

public class GetMessagesResponse
{
    public Guid ConversationId { get; set; }

    public List<MessageDto> Messages { get; set; } = [];

    public class MessageDto
    {
        public Guid Id { get; set; }

        public Guid SenderId { get; set; }

        public string SenderUsername { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime SentAt { get; set; }

        public bool IsRead { get; set; }
    }
}
