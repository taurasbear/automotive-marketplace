import type { Offer } from './Offer';

export type ReceiveMessagePayload = {
  id: string;
  conversationId: string;
  senderId: string;
  senderUsername: string;
  content: string;
  sentAt: string;
  isRead: boolean;
  messageType: 'Text' | 'Offer';
  offer?: Offer;
};
