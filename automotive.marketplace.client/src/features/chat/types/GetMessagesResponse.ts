import type { Message } from "./Message";

export type GetMessagesResponse = {
  conversationId: string;
  messages: Message[];
};
