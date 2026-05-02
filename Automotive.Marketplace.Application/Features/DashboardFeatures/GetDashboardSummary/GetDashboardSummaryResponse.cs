namespace Automotive.Marketplace.Application.Features.DashboardFeatures.GetDashboardSummary;

public sealed record GetDashboardSummaryResponse
{
    public OfferSummary Offers { get; set; } = new();
    public MeetingSummary Meetings { get; set; } = new();
    public ContractSummary Contracts { get; set; } = new();
    public AvailabilitySummary Availability { get; set; } = new();

    public sealed record OfferSummary
    {
        public int PendingCount { get; set; }
        public string? NewestOfferListing { get; set; }
        public decimal? NewestOfferAmount { get; set; }
        public string? NewestOfferFrom { get; set; }
    }

    public sealed record MeetingSummary
    {
        public int UpcomingCount { get; set; }
        public DateTime? NextMeetingAt { get; set; }
        public string? NextMeetingCounterpart { get; set; }
        public string? NextMeetingListing { get; set; }
    }

    public sealed record ContractSummary
    {
        public int ActionNeededCount { get; set; }
        public string? NextActionListing { get; set; }
        public string? NextActionType { get; set; }
    }

    public sealed record AvailabilitySummary
    {
        public int PendingCount { get; set; }
    }
}