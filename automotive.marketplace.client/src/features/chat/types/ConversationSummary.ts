export type ConversationSummary = {
  id: string;
  listingId: string;
  listingTitle: string;
  listingThumbnail: { url: string; altText: string } | null;
  listingPrice: number;
  listingMake: string;
  listingCommercialName: string;
  listingVin: string | null;
  listingMileage: number;
  counterpartId: string;
  counterpartUsername: string;
  lastMessage: string | null;
  lastMessageAt: string;
  unreadCount: number;
  buyerId: string;
  sellerId: string;
  buyerHasEngaged: boolean;
};
