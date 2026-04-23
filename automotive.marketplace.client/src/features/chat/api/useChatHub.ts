import { chatKeys } from "@/api/queryKeys/chatKeys";
import { selectAccessToken } from "@/features/auth";
import { useAppSelector } from "@/hooks/redux";
import queryClient from "@/lib/tanstack-query/queryClient";
import * as signalR from "@microsoft/signalr";
import type { AxiosResponse } from "axios";
import { useCallback, useEffect, useRef } from "react";
import { HUB_METHODS } from "../constants/chatHub";
import type { GetUnreadCountResponse } from "../api/getUnreadCountOptions";
import type {
  GetMessagesResponse,
  Message,
} from "../types/GetMessagesResponse";
import type { ReceiveMessagePayload } from "../types/ReceiveMessagePayload";
import type {
  OfferCounteredPayload,
  OfferExpiredPayload,
  OfferMadePayload,
  OfferStatusUpdatedPayload,
} from "../types/OfferEventPayloads";
import type {
  MeetingProposedPayload,
  MeetingStatusUpdatedPayload,
  MeetingRescheduledPayload,
  MeetingExpiredPayload,
  AvailabilitySharedPayload,
  AvailabilityRespondedPayload,
  AvailabilityExpiredPayload,
  MeetingCancelledPayload,
  AvailabilityCancelledPayload,
} from "../types/MeetingEventPayloads";

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

    connection.on(HUB_METHODS.OFFER_MADE, (payload: OfferMadePayload) => {
      queryClient.setQueryData<{ data: GetMessagesResponse }>(
        chatKeys.messages(payload.conversationId),
        (old) => {
          if (!old) return old;
          const newMessage: Message = {
            id: payload.messageId,
            senderId: payload.senderId,
            senderUsername: payload.senderUsername,
            content: "",
            sentAt: payload.sentAt,
            isRead: false,
            messageType: "Offer",
            offer: payload.offer,
          };
          return {
            ...old,
            data: { ...old.data, messages: [...old.data.messages, newMessage] },
          };
        },
      );
      void queryClient.invalidateQueries({
        queryKey: chatKeys.conversations(),
      });
    });

    const handleOfferStatusUpdate = (payload: OfferStatusUpdatedPayload) => {
      queryClient.setQueryData<{ data: GetMessagesResponse }>(
        chatKeys.messages(payload.conversationId),
        (old) => {
          if (!old) return old;
          return {
            ...old,
            data: {
              ...old.data,
              messages: old.data.messages.map((m) =>
                m.offer?.id === payload.offerId
                  ? { ...m, offer: { ...m.offer, status: payload.newStatus } }
                  : m,
              ),
            },
          };
        },
      );
    };

    connection.on(HUB_METHODS.OFFER_ACCEPTED, handleOfferStatusUpdate);
    connection.on(HUB_METHODS.OFFER_DECLINED, handleOfferStatusUpdate);

    connection.on(
      HUB_METHODS.OFFER_COUNTERED,
      (payload: OfferCounteredPayload) => {
        queryClient.setQueryData<{ data: GetMessagesResponse }>(
          chatKeys.messages(payload.conversationId),
          (old) => {
            if (!old) return old;
            const updatedMessages = old.data.messages.map((m) =>
              m.offer?.id === payload.offerId
                ? { ...m, offer: { ...m.offer, status: "Countered" as const } }
                : m,
            );
            const counterMessage: Message = {
              id: payload.counterOffer.messageId,
              senderId: payload.counterOffer.senderId,
              senderUsername: payload.counterOffer.senderUsername,
              content: "",
              sentAt: payload.counterOffer.sentAt,
              isRead: false,
              messageType: "Offer",
              offer: payload.counterOffer.offer,
            };
            return {
              ...old,
              data: {
                ...old.data,
                messages: [...updatedMessages, counterMessage],
              },
            };
          },
        );
        void queryClient.invalidateQueries({
          queryKey: chatKeys.conversations(),
        });
      },
    );

    connection.on(HUB_METHODS.OFFER_EXPIRED, (payload: OfferExpiredPayload) => {
      queryClient.setQueryData<{ data: GetMessagesResponse }>(
        chatKeys.messages(payload.conversationId),
        (old) => {
          if (!old) return old;
          return {
            ...old,
            data: {
              ...old.data,
              messages: old.data.messages.map((m) =>
                m.offer?.id === payload.offerId
                  ? { ...m, offer: { ...m.offer, status: "Expired" as const } }
                  : m,
              ),
            },
          };
        },
      );
    });

    connection.on(
      HUB_METHODS.MEETING_PROPOSED,
      (payload: MeetingProposedPayload) => {
        queryClient.setQueryData<{ data: GetMessagesResponse }>(
          chatKeys.messages(payload.conversationId),
          (old) => {
            if (!old) return old;
            const newMessage: Message = {
              id: payload.messageId,
              senderId: payload.senderId,
              senderUsername: payload.senderUsername,
              content: "",
              sentAt: payload.sentAt,
              isRead: false,
              messageType: "Meeting",
              meeting: payload.meeting,
            };
            return {
              ...old,
              data: {
                ...old.data,
                messages: [...old.data.messages, newMessage],
              },
            };
          },
        );
        void queryClient.invalidateQueries({
          queryKey: chatKeys.conversations(),
        });
      },
    );

    const handleMeetingStatusUpdate = (
      payload: MeetingStatusUpdatedPayload,
    ) => {
      queryClient.setQueryData<{ data: GetMessagesResponse }>(
        chatKeys.messages(payload.conversationId),
        (old) => {
          if (!old) return old;
          return {
            ...old,
            data: {
              ...old.data,
              messages: old.data.messages.map((m) =>
                m.meeting?.id === payload.meetingId
                  ? {
                      ...m,
                      meeting: { ...m.meeting, status: payload.newStatus },
                    }
                  : m,
              ),
            },
          };
        },
      );
    };

    connection.on(HUB_METHODS.MEETING_ACCEPTED, handleMeetingStatusUpdate);
    connection.on(HUB_METHODS.MEETING_DECLINED, handleMeetingStatusUpdate);

    connection.on(
      HUB_METHODS.MEETING_RESCHEDULED,
      (payload: MeetingRescheduledPayload) => {
        queryClient.setQueryData<{ data: GetMessagesResponse }>(
          chatKeys.messages(payload.conversationId),
          (old) => {
            if (!old) return old;
            const updatedMessages = old.data.messages.map((m) =>
              m.meeting?.id === payload.meetingId
                ? {
                    ...m,
                    meeting: { ...m.meeting, status: "Rescheduled" as const },
                  }
                : m,
            );
            const rescheduledMessage: Message = {
              id: payload.rescheduledMeeting.messageId,
              senderId: payload.rescheduledMeeting.senderId,
              senderUsername: payload.rescheduledMeeting.senderUsername,
              content: "",
              sentAt: payload.rescheduledMeeting.sentAt,
              isRead: false,
              messageType: "Meeting",
              meeting: payload.rescheduledMeeting.meeting,
            };
            return {
              ...old,
              data: {
                ...old.data,
                messages: [...updatedMessages, rescheduledMessage],
              },
            };
          },
        );
        void queryClient.invalidateQueries({
          queryKey: chatKeys.conversations(),
        });
      },
    );

    connection.on(
      HUB_METHODS.MEETING_EXPIRED,
      (payload: MeetingExpiredPayload) => {
        queryClient.setQueryData<{ data: GetMessagesResponse }>(
          chatKeys.messages(payload.conversationId),
          (old) => {
            if (!old) return old;
            return {
              ...old,
              data: {
                ...old.data,
                messages: old.data.messages.map((m) =>
                  m.meeting?.id === payload.meetingId
                    ? {
                        ...m,
                        meeting: { ...m.meeting, status: "Expired" as const },
                      }
                    : m,
                ),
              },
            };
          },
        );
      },
    );

    connection.on(
      HUB_METHODS.MEETING_CANCELLED,
      (payload: MeetingCancelledPayload) => {
        queryClient.setQueryData<{ data: GetMessagesResponse }>(
          chatKeys.messages(payload.conversationId),
          (old) => {
            if (!old) return old;
            return {
              ...old,
              data: {
                ...old.data,
                messages: old.data.messages.map((m) =>
                  m.meeting?.id === payload.meetingId
                    ? {
                        ...m,
                        meeting: { ...m.meeting, status: "Cancelled" as const },
                      }
                    : m,
                ),
              },
            };
          },
        );
      },
    );

    connection.on(
      HUB_METHODS.AVAILABILITY_SHARED,
      (payload: AvailabilitySharedPayload) => {
        queryClient.setQueryData<{ data: GetMessagesResponse }>(
          chatKeys.messages(payload.conversationId),
          (old) => {
            if (!old) return old;
            const newMessage: Message = {
              id: payload.messageId,
              senderId: payload.senderId,
              senderUsername: payload.senderUsername,
              content: "",
              sentAt: payload.sentAt,
              isRead: false,
              messageType: "Availability",
              availabilityCard: payload.availabilityCard,
            };
            return {
              ...old,
              data: {
                ...old.data,
                messages: [...old.data.messages, newMessage],
              },
            };
          },
        );
        void queryClient.invalidateQueries({
          queryKey: chatKeys.conversations(),
        });
      },
    );

    connection.on(
      HUB_METHODS.AVAILABILITY_RESPONDED,
      (payload: AvailabilityRespondedPayload) => {
        queryClient.setQueryData<{ data: GetMessagesResponse }>(
          chatKeys.messages(payload.conversationId),
          (old) => {
            if (!old) return old;
            const updatedMessages = old.data.messages.map((m) =>
              m.availabilityCard?.id === payload.availabilityCardId
                ? {
                    ...m,
                    availabilityCard: {
                      ...m.availabilityCard,
                      status: "Responded" as const,
                    },
                  }
                : m,
            );
            if (payload.action === "PickSlot" && payload.pickedSlotMeeting) {
              const meetingMessage: Message = {
                id: payload.pickedSlotMeeting.messageId,
                senderId: payload.pickedSlotMeeting.senderId,
                senderUsername: payload.pickedSlotMeeting.senderUsername,
                content: "",
                sentAt: payload.pickedSlotMeeting.sentAt,
                isRead: false,
                messageType: "Meeting",
                meeting: payload.pickedSlotMeeting.meeting,
              };
              return {
                ...old,
                data: {
                  ...old.data,
                  messages: [...updatedMessages, meetingMessage],
                },
              };
            }
            if (
              payload.action === "ShareBack" &&
              payload.sharedBackAvailability
            ) {
              const availMessage: Message = {
                id: payload.sharedBackAvailability.messageId,
                senderId: payload.sharedBackAvailability.senderId,
                senderUsername: payload.sharedBackAvailability.senderUsername,
                content: "",
                sentAt: payload.sharedBackAvailability.sentAt,
                isRead: false,
                messageType: "Availability",
                availabilityCard:
                  payload.sharedBackAvailability.availabilityCard,
              };
              return {
                ...old,
                data: {
                  ...old.data,
                  messages: [...updatedMessages, availMessage],
                },
              };
            }
            return { ...old, data: { ...old.data, messages: updatedMessages } };
          },
        );
        void queryClient.invalidateQueries({
          queryKey: chatKeys.conversations(),
        });
      },
    );

    connection.on(
      HUB_METHODS.AVAILABILITY_EXPIRED,
      (payload: AvailabilityExpiredPayload) => {
        queryClient.setQueryData<{ data: GetMessagesResponse }>(
          chatKeys.messages(payload.conversationId),
          (old) => {
            if (!old) return old;
            return {
              ...old,
              data: {
                ...old.data,
                messages: old.data.messages.map((m) =>
                  m.availabilityCard?.id === payload.availabilityCardId
                    ? {
                        ...m,
                        availabilityCard: {
                          ...m.availabilityCard,
                          status: "Expired" as const,
                        },
                      }
                    : m,
                ),
              },
            };
          },
        );
      },
    );

    connection.on(
      HUB_METHODS.AVAILABILITY_CANCELLED,
      (payload: AvailabilityCancelledPayload) => {
        queryClient.setQueryData<{ data: GetMessagesResponse }>(
          chatKeys.messages(payload.conversationId),
          (old) => {
            if (!old) return old;
            return {
              ...old,
              data: {
                ...old.data,
                messages: old.data.messages.map((m) =>
                  m.availabilityCard?.id === payload.availabilityCardId
                    ? {
                        ...m,
                        availabilityCard: {
                          ...m.availabilityCard,
                          status: "Cancelled" as const,
                        },
                      }
                    : m,
                ),
              },
            };
          },
        );
      },
    );

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

  const sendOffer = useCallback(
    ({
      conversationId,
      amount,
    }: {
      conversationId: string;
      amount: number;
    }) => {
      if (
        connectionRef.current?.state !== signalR.HubConnectionState.Connected
      ) {
        throw new Error("Not connected. Please wait and try again.");
      }
      void connectionRef.current.invoke(
        HUB_METHODS.MAKE_OFFER,
        conversationId,
        amount,
      );
    },
    [],
  );

  const respondToOffer = useCallback(
    ({
      offerId,
      action,
      counterAmount,
    }: {
      offerId: string;
      action: "Accept" | "Decline" | "Counter";
      counterAmount?: number;
    }) => {
      if (
        connectionRef.current?.state !== signalR.HubConnectionState.Connected
      ) {
        throw new Error("Not connected. Please wait and try again.");
      }
      void connectionRef.current.invoke(
        HUB_METHODS.RESPOND_TO_OFFER,
        offerId,
        action,
        counterAmount ?? null,
      );
    },
    [],
  );

  const proposeMeeting = useCallback(
    ({
      conversationId,
      proposedAt,
      durationMinutes,
      locationText,
      locationLat,
      locationLng,
    }: {
      conversationId: string;
      proposedAt: string;
      durationMinutes: number;
      locationText?: string;
      locationLat?: number;
      locationLng?: number;
    }) => {
      if (
        connectionRef.current?.state !== signalR.HubConnectionState.Connected
      ) {
        throw new Error("Not connected. Please wait and try again.");
      }
      void connectionRef.current.invoke(
        HUB_METHODS.PROPOSE_MEETING,
        conversationId,
        proposedAt,
        durationMinutes,
        locationText ?? null,
        locationLat ?? null,
        locationLng ?? null,
      );
    },
    [],
  );

  const respondToMeeting = useCallback(
    ({
      meetingId,
      action,
      rescheduleData,
    }: {
      meetingId: string;
      action: "Accept" | "Decline" | "Reschedule";
      rescheduleData?: {
        proposedAt: string;
        durationMinutes: number;
        locationText?: string;
        locationLat?: number;
        locationLng?: number;
      };
    }) => {
      if (
        connectionRef.current?.state !== signalR.HubConnectionState.Connected
      ) {
        throw new Error("Not connected. Please wait and try again.");
      }
      void connectionRef.current.invoke(
        HUB_METHODS.RESPOND_TO_MEETING,
        meetingId,
        action,
        rescheduleData ?? null,
      );
    },
    [],
  );

  const shareAvailability = useCallback(
    ({
      conversationId,
      slots,
    }: {
      conversationId: string;
      slots: { startTime: string; endTime: string }[];
    }) => {
      if (
        connectionRef.current?.state !== signalR.HubConnectionState.Connected
      ) {
        throw new Error("Not connected. Please wait and try again.");
      }
      void connectionRef.current.invoke(
        HUB_METHODS.SHARE_AVAILABILITY,
        conversationId,
        slots,
      );
    },
    [],
  );

  const respondToAvailability = useCallback(
    ({
      availabilityCardId,
      action,
      slotId,
      shareBackSlots,
      startTime,
      durationMinutes,
    }: {
      availabilityCardId: string;
      action: "PickSlot" | "ShareBack";
      slotId?: string;
      shareBackSlots?: { startTime: string; endTime: string }[];
      startTime?: string;
      durationMinutes?: number;
    }) => {
      if (
        connectionRef.current?.state !== signalR.HubConnectionState.Connected
      ) {
        throw new Error("Not connected. Please wait and try again.");
      }
      void connectionRef.current.invoke(
        HUB_METHODS.RESPOND_TO_AVAILABILITY,
        availabilityCardId,
        action,
        slotId ?? null,
        shareBackSlots ?? null,
        startTime ?? null,
        durationMinutes ?? null,
      );
    },
    [],
  );

  const cancelMeeting = useCallback(({ meetingId }: { meetingId: string }) => {
    if (connectionRef.current?.state !== signalR.HubConnectionState.Connected) {
      throw new Error("Not connected. Please wait and try again.");
    }
    void connectionRef.current.invoke(HUB_METHODS.CANCEL_MEETING, meetingId);
  }, []);

  const cancelAvailability = useCallback(
    ({ availabilityCardId }: { availabilityCardId: string }) => {
      if (
        connectionRef.current?.state !== signalR.HubConnectionState.Connected
      ) {
        throw new Error("Not connected. Please wait and try again.");
      }
      void connectionRef.current.invoke(
        HUB_METHODS.CANCEL_AVAILABILITY,
        availabilityCardId,
      );
    },
    [],
  );

  return {
    sendMessage,
    sendOffer,
    respondToOffer,
    proposeMeeting,
    respondToMeeting,
    shareAvailability,
    respondToAvailability,
    cancelMeeting,
    cancelAvailability,
  };
};
