import { chatKeys } from "@/api/queryKeys/chatKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";
import type { GetMessagesQuery } from "../types/GetMessagesQuery";
import type { GetMessagesResponse } from "../types/GetMessagesResponse";

const getMessages = (query: GetMessagesQuery) =>
  axiosClient.get<GetMessagesResponse>(ENDPOINTS.CHAT.GET_MESSAGES, {
    params: query,
  });

export const getMessagesOptions = (query: GetMessagesQuery) =>
  queryOptions({
    queryKey: chatKeys.messages(query.conversationId),
    queryFn: () => getMessages(query),
  });
