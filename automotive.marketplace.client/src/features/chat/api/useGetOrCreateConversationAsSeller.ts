import { chatKeys } from "@/api/queryKeys/chatKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation } from "@tanstack/react-query";

interface GetOrCreateConversationAsSellerCommand {
  listingId: string;
  buyerId: string;
}

interface GetOrCreateConversationAsSellerResponse {
  conversationId: string;
}

const getOrCreateConversationAsSeller = (
  command: GetOrCreateConversationAsSellerCommand,
) =>
  axiosClient.post<GetOrCreateConversationAsSellerResponse>(
    ENDPOINTS.CHAT.GET_OR_CREATE_CONVERSATION_AS_SELLER,
    command,
  );

export const useGetOrCreateConversationAsSeller = () =>
  useMutation({
    mutationFn: getOrCreateConversationAsSeller,
    meta: {
      errorMessage: "toasts:chat.conversationError",
      invalidatesQuery: chatKeys.conversations(),
    },
  });
