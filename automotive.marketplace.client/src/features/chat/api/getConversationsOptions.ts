import { chatKeys } from "@/api/queryKeys/chatKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { ConversationSummary } from "../types/ConversationSummary";

const getConversations = () =>
  axiosClient.get<ConversationSummary[]>(ENDPOINTS.CHAT.GET_CONVERSATIONS);

export const getConversationsOptions = () =>
  queryOptions({
    queryKey: chatKeys.conversations(),
    queryFn: () => getConversations(),
  });
