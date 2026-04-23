import type { Offer } from "./Offer";
import type { Meeting } from "./Meeting";
import type { AvailabilityCard } from "./AvailabilityCard";

export type ReceiveMessagePayload = {
  id: string;
  conversationId: string;
  senderId: string;
  senderUsername: string;
  content: string;
  sentAt: string;
  isRead: boolean;
  messageType: "Text" | "Offer" | "Meeting" | "Availability";
  offer?: Offer;
  meeting?: Meeting;
  availabilityCard?: AvailabilityCard;
};
