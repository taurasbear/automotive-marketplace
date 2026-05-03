export type GetDashboardSummaryResponse = {
  offers: {
    pendingCount: number;
    newestOfferListing: string | null;
    newestOfferAmount: number | null;
    newestOfferFrom: string | null;
  };
  meetings: {
    upcomingCount: number;
    nextMeetingAt: string | null;
    nextMeetingCounterpart: string | null;
    nextMeetingListing: string | null;
  };
  contracts: {
    actionNeededCount: number;
    nextActionListing: string | null;
    nextActionType: string | null;
  };
  availability: {
    pendingCount: number;
  };
};