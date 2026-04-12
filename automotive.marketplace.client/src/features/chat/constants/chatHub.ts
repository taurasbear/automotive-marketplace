export const HUB_METHODS = {
  SEND_MESSAGE: "SendMessage",
  RECEIVE_MESSAGE: "ReceiveMessage",
  UPDATE_UNREAD_COUNT: "UpdateUnreadCount",
} as const;

export type ReceiveMessagePayload = {
  id: string;
  conversationId: string;
  senderId: string;
  senderUsername: string;
  content: string;
  sentAt: string;
  isRead: boolean;
};
