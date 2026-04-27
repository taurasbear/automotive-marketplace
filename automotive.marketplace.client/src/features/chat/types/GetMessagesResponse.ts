import type { Offer } from "./Offer";
import type { Meeting } from "./Meeting";
import type { AvailabilityCard } from "./AvailabilityCard";
import type { ContractCard } from "./ContractCard";

export type GetMessagesResponse = {
  conversationId: string;
  messages: Message[];
};

export type Message = {
  id: string;
  senderId: string;
  senderUsername: string;
  content: string;
  sentAt: string;
  isRead: boolean;
  messageType: "Text" | "Offer" | "Meeting" | "Availability" | "Contract";
  offer?: Offer;
  meeting?: Meeting;
  availabilityCard?: AvailabilityCard;
  contractCard?: ContractCard;
};
