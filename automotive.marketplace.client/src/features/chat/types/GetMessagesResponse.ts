import type { Offer } from './Offer';

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
  messageType: 'Text' | 'Offer';
  offer?: Offer;
};
