import { chatKeys } from "@/api/queryKeys/chatKeys";
import { selectAccessToken } from "@/features/auth";
import { useAppDispatch, useAppSelector } from "@/hooks/redux";
import queryClient from "@/lib/tanstack-query/queryClient";
import * as signalR from "@microsoft/signalr";
import { useQuery } from "@tanstack/react-query";
import { useCallback, useEffect, useRef } from "react";
import { getUnreadCountOptions } from "./getUnreadCountOptions";
import { HUB_METHODS, type ReceiveMessagePayload } from "../constants/chatHub";
import { setUnreadCount } from "../state/chatSlice";
import type { GetMessagesResponse } from "../types/GetMessagesResponse";

const apiBase =
  (import.meta.env.VITE_APP_API_URL as string) ||
  "https://api.automotive-marketplace.taurasbear.me";
const hubUrl = import.meta.env.PROD ? `${apiBase}/hubs/chat` : "/api/hubs/chat";

const connectionRef = { current: null as signalR.HubConnection | null };

export const useChatHub = () => {
  const accessToken = useAppSelector(selectAccessToken);
  const dispatch = useAppDispatch();
  const isOwner = useRef(false);

  const { data: unreadQuery } = useQuery({
    ...getUnreadCountOptions(),
    enabled: !!accessToken,
  });

  useEffect(() => {
    if (unreadQuery?.data.unreadCount !== undefined) {
      dispatch(setUnreadCount(unreadQuery.data.unreadCount));
    }
  }, [unreadQuery?.data.unreadCount, dispatch]);

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
        void queryClient.invalidateQueries({
          queryKey: chatKeys.unreadCount(),
        });
      },
    );

    connection.on(HUB_METHODS.UPDATE_UNREAD_COUNT, (count: number) => {
      dispatch(setUnreadCount(count));
    });

    connectionRef.current = connection;
    connection.start().catch(console.error);

    return () => {
      if (isOwner.current) {
        void connection.stop();
        connectionRef.current = null;
      }
    };
  }, [accessToken, dispatch]);

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
