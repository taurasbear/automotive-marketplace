namespace Automotive.Marketplace.Application.Features.ChatFeatures.MarkMessagesRead;

public sealed record MarkMessagesReadResponse
{
    public int TotalUnreadCount { get; set; }
}
