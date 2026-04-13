namespace Automotive.Marketplace.Application.Features.ChatFeatures.SendMessage;

public sealed record SendMessageResponse
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public Guid SenderId { get; set; }

    public string SenderUsername { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public DateTime SentAt { get; set; }

    public Guid RecipientId { get; set; }

    public int RecipientUnreadCount { get; set; }

    public bool IsRead { get; set; } = false;
}
