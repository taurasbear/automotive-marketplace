import { chatKeys } from "@/api/queryKeys/chatKeys";
import { ENDPOINTS } from "@/constants/endpoints";
import axiosClient from "@/lib/axios/axiosClient";
import { queryOptions } from "@tanstack/react-query";

type GetUnreadCountResponse = {
  unreadCount: number;
};

const getUnreadCount = () =>
  axiosClient.get<GetUnreadCountResponse>(ENDPOINTS.CHAT.GET_UNREAD_COUNT);

export const getUnreadCountOptions = () =>
  queryOptions({
    queryKey: chatKeys.unreadCount(),
    queryFn: () => getUnreadCount(),
  });
