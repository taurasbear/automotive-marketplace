import { chatKeys } from "@/api/queryKeys/chatKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";
import type { GetOrCreateConversationCommand } from "../types/GetOrCreateConversationCommand";
import type { GetOrCreateConversationResponse } from "../types/GetOrCreateConversationResponse";

const getOrCreateConversation = (command: GetOrCreateConversationCommand) =>
  axiosClient.post<GetOrCreateConversationResponse>(
    ENDPOINTS.CHAT.GET_OR_CREATE_CONVERSATION,
    command,
  );

export const useGetOrCreateConversation = () =>
  useMutation({
    mutationFn: getOrCreateConversation,
    meta: {
      errorMessage: "Could not open conversation. Please try again.",
      invalidatesQuery: chatKeys.conversations(),
    },
  });
