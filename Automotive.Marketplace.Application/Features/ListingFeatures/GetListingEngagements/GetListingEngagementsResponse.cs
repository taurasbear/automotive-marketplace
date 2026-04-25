namespace Automotive.Marketplace.Application.Features.ListingFeatures.GetListingEngagements;

public sealed record GetListingEngagementsResponse
{
    public IEnumerable<Conversation> Conversations { get; set; } = [];
    public IEnumerable<Liker> Likers { get; set; } = [];

    public sealed record Conversation
    {
        public Guid ConversationId { get; set; }
        public Guid BuyerId { get; set; }
        public string BuyerUsername { get; set; } = string.Empty;
        public string LastMessageType { get; set; } = string.Empty;
        public DateTime LastInteractionAt { get; set; }
    }

    public sealed record Liker
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public DateTime LikedAt { get; set; }
    }
}
