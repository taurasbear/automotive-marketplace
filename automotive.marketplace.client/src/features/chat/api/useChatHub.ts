import { chatKeys } from "@/api/queryKeys/chatKeys";
import { selectAccessToken } from "@/features/auth";
import { useAppSelector } from "@/hooks/redux";
import queryClient from "@/lib/tanstack-query/queryClient";
import * as signalR from "@microsoft/signalr";
import type { AxiosResponse } from "axios";
import { useCallback, useEffect, useRef } from "react";
import { HUB_METHODS } from "../constants/chatHub";
import type { GetUnreadCountResponse } from "../api/getUnreadCountOptions";
import type { GetMessagesResponse } from "../types/GetMessagesResponse";
import type { ReceiveMessagePayload } from "../types/ReceiveMessagePayload";

const apiBase =
  (import.meta.env.VITE_APP_API_URL as string) ||
  "https://api.automotive-marketplace.taurasbear.me";
const hubUrl = import.meta.env.PROD ? `${apiBase}/hubs/chat` : "/api/hubs/chat";

const connectionRef = { current: null as signalR.HubConnection | null };

export const useChatHub = () => {
  const accessToken = useAppSelector(selectAccessToken);
  const isOwner = useRef(false);

  useEffect(() => {
    if (!accessToken) return;
    if (connectionRef.current) return;

    isOwner.current = true;
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => accessToken })
      .withAutomaticReconnect()
      .build();

    connection.on(
      HUB_METHODS.RECEIVE_MESSAGE,
      (message: ReceiveMessagePayload) => {
        queryClient.setQueryData<{ data: GetMessagesResponse }>(
          chatKeys.messages(message.conversationId),
          (old) => {
            if (!old) return old;
            return {
              ...old,
              data: { ...old.data, messages: [...old.data.messages, message] },
            };
          },
        );
        void queryClient.invalidateQueries({
          queryKey: chatKeys.conversations(),
        });
      },
    );

    connection.on(HUB_METHODS.UPDATE_UNREAD_COUNT, (count: number) => {
      queryClient.setQueryData<AxiosResponse<GetUnreadCountResponse>>(
        chatKeys.unreadCount(),
        (old) => {
          if (!old) return old;
          return { ...old, data: { unreadCount: count } };
        },
      );
    });

    connectionRef.current = connection;
    connection.start().catch(console.error);

    return () => {
      if (isOwner.current) {
        void connection.stop();
        connectionRef.current = null;
      }
    };
  }, [accessToken]);

  const sendMessage = useCallback(
    ({
      conversationId,
      content,
    }: {
      conversationId: string;
      content: string;
    }) => {
      if (
        connectionRef.current?.state !== signalR.HubConnectionState.Connected
      ) {
        throw new Error("Not connected. Please wait and try again.");
      }
      void connectionRef.current.invoke(
        HUB_METHODS.SEND_MESSAGE,
        conversationId,
        content,
      );
    },
    [],
  );

  return { sendMessage };
};
