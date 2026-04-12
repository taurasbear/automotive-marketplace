export type GetMessagesResponse = {
  conversationId: string;
  messages: Message[];
};

type Message = {
  id: string;
  senderId: string;
  senderUsername: string;
  content: string;
  sentAt: string;
  isRead: boolean;
};
