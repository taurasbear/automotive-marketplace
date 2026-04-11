import { chatKeys } from "@/api/queryKeys/chatKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { useMutation, useQueryClient } from "@tanstack/react-query";

const markMessagesRead = (conversationId: string) =>
  axiosClient.put<void>(ENDPOINTS.CHAT.MARK_MESSAGES_READ, { conversationId });

export const useMarkMessagesRead = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: markMessagesRead,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: chatKeys.conversations() });
    },
  });
};
