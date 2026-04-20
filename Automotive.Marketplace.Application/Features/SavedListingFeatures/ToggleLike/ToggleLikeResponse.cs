namespace Automotive.Marketplace.Application.Features.SavedListingFeatures.ToggleLike;

public sealed record ToggleLikeResponse
{
    public bool IsLiked { get; set; }
}
