namespace Automotive.Marketplace.Application.Features.ChatFeatures.GetConversations;

public sealed record ConversationSummaryResponse
{
    public Guid Id { get; set; }

    public Guid ListingId { get; set; }

    public string ListingTitle { get; set; } = string.Empty;

    public Automotive.Marketplace.Application.Models.ImageDto? ListingThumbnail { get; set; }

    public decimal ListingPrice { get; set; }

    public Guid CounterpartId { get; set; }

    public string CounterpartUsername { get; set; } = string.Empty;

    public string? LastMessage { get; set; }

    public DateTime LastMessageAt { get; set; }

    public int UnreadCount { get; set; }
}
