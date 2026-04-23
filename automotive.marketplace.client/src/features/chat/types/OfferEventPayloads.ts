import type { Offer } from "./Offer";

export type OfferMadePayload = {
  messageId: string;
  conversationId: string;
  senderId: string;
  senderUsername: string;
  sentAt: string;
  recipientId: string;
  offer: Offer;
};

export type OfferStatusUpdatedPayload = {
  offerId: string;
  conversationId: string;
  newStatus: "Accepted" | "Declined";
  initiatorId: string;
  responderId: string;
  counterOffer: null;
};

export type OfferCounteredPayload = {
  offerId: string;
  conversationId: string;
  newStatus: "Countered";
  initiatorId: string;
  responderId: string;
  counterOffer: OfferMadePayload;
};

export type OfferExpiredPayload = {
  offerId: string;
  conversationId: string;
};
