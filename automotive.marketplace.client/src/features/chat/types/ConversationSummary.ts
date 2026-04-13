export type ConversationSummary = {
  id: string;
  listingId: string;
  listingTitle: string;
  listingThumbnailUrl: string | null;
  listingPrice: number;
  counterpartId: string;
  counterpartUsername: string;
  lastMessage: string | null;
  lastMessageAt: string;
  unreadCount: number;
};
