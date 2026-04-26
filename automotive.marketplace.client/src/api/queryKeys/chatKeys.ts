export const chatKeys = {
  all: () => ["chat"] as const,
  conversations: () => [...chatKeys.all(), "conversations"] as const,
  messages: (conversationId: string) =>
    [...chatKeys.all(), "messages", conversationId] as const,
  unreadCount: () => [...chatKeys.all(), "unreadCount"] as const,
  contractCard: (contractCardId: string) =>
    [...chatKeys.all(), "contractCard", contractCardId] as const,
  userContractProfile: () => [...chatKeys.all(), "userContractProfile"] as const,
};
