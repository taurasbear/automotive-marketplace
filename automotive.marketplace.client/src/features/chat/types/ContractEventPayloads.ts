import type { ContractCardStatus } from "./ContractCard";

export type ContractRequestedPayload = {
  messageId: string;
  conversationId: string;
  senderId: string;
  senderUsername: string;
  sentAt: string;
  recipientId: string;
  contractCard: {
    id: string;
    status: ContractCardStatus;
    initiatorId: string;
    createdAt: string;
  };
};

export type ContractStatusUpdatedPayload = {
  contractCardId: string;
  conversationId: string;
  newStatus: ContractCardStatus;
  sellerSubmittedAt?: string;
  buyerSubmittedAt?: string;
};
