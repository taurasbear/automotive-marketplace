import { chatKeys } from "@/api/queryKeys/chatKeys";
import { selectAccessToken } from "@/features/auth";
import { useAppDispatch, useAppSelector } from "@/hooks/redux";
import * as signalR from "@microsoft/signalr";
import { useQueryClient } from "@tanstack/react-query";
import { useCallback, useEffect, useRef } from "react";
import { setUnreadCount } from "../state/chatSlice";
import type { GetMessagesResponse } from "../types/GetMessagesResponse";
import type { Message } from "../types/Message";

const connectionRef = { current: null as signalR.HubConnection | null };

export const useChatHub = () => {
  const accessToken = useAppSelector(selectAccessToken);
  const dispatch = useAppDispatch();
  const queryClient = useQueryClient();
  const isOwner = useRef(false);

  useEffect(() => {
    if (!accessToken) return;
    if (connectionRef.current) return;

    isOwner.current = true;
    const connection = new signalR.HubConnectionBuilder()
      .withUrl("/api/hubs/chat", { accessTokenFactory: () => accessToken })
      .withAutomaticReconnect()
      .build();

    connection.on("ReceiveMessage", (message: Message) => {
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
    });

    connection.on("UpdateUnreadCount", (count: number) => {
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
  }, [accessToken, dispatch, queryClient]);

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
      void connectionRef.current.invoke("SendMessage", conversationId, content);
    },
    [],
  );

  return { sendMessage };
};
