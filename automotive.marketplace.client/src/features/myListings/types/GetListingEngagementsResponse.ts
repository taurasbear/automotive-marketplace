export type ListingConversationEngagement = {
  conversationId: string;
  buyerId: string;
  buyerUsername: string;
  lastMessageType: "Text" | "Offer" | "Meeting" | "Availability";
  lastInteractionAt: string;
};

export type ListingLikerEngagement = {
  userId: string;
  username: string;
  likedAt: string;
};

export type GetListingEngagementsResponse = {
  conversations: ListingConversationEngagement[];
  likers: ListingLikerEngagement[];
};
