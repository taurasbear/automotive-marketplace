import { Button } from "@/components/ui/button";
import { useAppSelector } from "@/hooks/redux";
import { useSuspenseQuery } from "@tanstack/react-query";
import { format } from "date-fns";
import { CheckCircle, MapPin } from "lucide-react";
import { useCallback, useEffect, useRef, useState } from "react";
import { getMessagesOptions } from "../api/getMessagesOptions";
import { useChatHub } from "../api/useChatHub";
import { useMarkMessagesRead } from "../api/useMarkMessagesRead";
import type { ConversationSummary } from "../types/ConversationSummary";
import { getTimezoneOffsetLabel } from "../utils/timezone";
import ActionBar from "./ActionBar";
import AvailabilityCardComponent from "./AvailabilityCardComponent";
import ListingCard from "./ListingCard";
import MeetingCard from "./MeetingCard";
import OfferCard from "./OfferCard";

type MessageThreadProps = {
  conversation: ConversationSummary;
  showListingCard?: boolean;
};

const MessageThread = ({
  conversation,
  showListingCard = true,
}: MessageThreadProps) => {
  const userId = useAppSelector((s) => s.auth.userId) ?? "";
  const { data: messagesQuery } = useSuspenseQuery(
    getMessagesOptions({ conversationId: conversation.id }),
  );
  const messages = messagesQuery.data.messages;
  const {
    sendMessage,
    sendOffer,
    respondToOffer,
    proposeMeeting,
    respondToMeeting,
    shareAvailability,
    respondToAvailability,
    cancelMeeting,
    cancelAvailability,
  } = useChatHub();
  const { mutate: markRead } = useMarkMessagesRead();
  const [input, setInput] = useState("");
  const [sendError, setSendError] = useState<string | null>(null);
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages.length]);

  useEffect(() => {
    markRead(conversation.id);
  }, [messages.length, conversation.id, markRead]);

  const hasActiveOffer = messages.some(
    (m) => m.messageType === "Offer" && m.offer?.status === "Pending",
  );

  const hasActiveMeeting = messages.some(
    (m) =>
      (m.messageType === "Meeting" && m.meeting?.status === "Pending") ||
      (m.messageType === "Availability" &&
        m.availabilityCard?.status === "Pending"),
  );

  const acceptedMeeting =
    messages.find(
      (m) => m.messageType === "Meeting" && m.meeting?.status === "Accepted",
    )?.meeting ?? null;

  const [showStickyBar, setShowStickyBar] = useState(false);
  const acceptedCardRef = useRef<HTMLDivElement>(null);
  const scrollContainerRef = useRef<HTMLDivElement>(null);

  const acceptedCardRefCallback = useCallback((node: HTMLDivElement | null) => {
    acceptedCardRef.current = node;
  }, []);

  useEffect(() => {
    const node = acceptedCardRef.current;
    const container = scrollContainerRef.current;
    if (!node || !container || !acceptedMeeting) {
      setShowStickyBar(false);
      return;
    }

    const observer = new IntersectionObserver(
      ([entry]) => {
        setShowStickyBar(!entry.isIntersecting);
      },
      { root: container, threshold: 0 },
    );

    observer.observe(node);
    return () => observer.disconnect();
  }, [acceptedMeeting]);

  const handleSend = () => {
    const trimmed = input.trim();
    if (!trimmed) return;
    try {
      sendMessage({ conversationId: conversation.id, content: trimmed });
      setInput("");
      setSendError(null);
    } catch (err) {
      setSendError(
        err instanceof Error ? err.message : "Failed to send message.",
      );
    }
  };

  return (
    <div className="flex h-full flex-col">
      {showListingCard && (
        <ListingCard
          listingId={conversation.listingId}
          listingTitle={conversation.listingTitle}
          listingPrice={conversation.listingPrice}
          listingThumbnail={conversation.listingThumbnail}
        />
      )}
      <div ref={scrollContainerRef} className="relative flex-1 overflow-y-auto">
        {showStickyBar && acceptedMeeting && (
          <div className="sticky top-0 z-10 flex items-center gap-2 bg-green-900/95 px-4 py-2 text-xs text-green-100 backdrop-blur-sm">
            <CheckCircle className="h-3.5 w-3.5 shrink-0" />
            <span className="font-semibold">Meetup Confirmed</span>
            <span className="text-green-300">·</span>
            <span>
              {format(new Date(acceptedMeeting.proposedAt), "EEE, MMM d")} ·{" "}
              {format(new Date(acceptedMeeting.proposedAt), "HH:mm")}–
              {format(
                new Date(
                  new Date(acceptedMeeting.proposedAt).getTime() +
                    acceptedMeeting.durationMinutes * 60000,
                ),
                "HH:mm",
              )}{" "}
              {getTimezoneOffsetLabel()}
            </span>
            {acceptedMeeting.locationText && (
              <>
                <span className="text-green-300">·</span>
                <MapPin className="h-3 w-3 shrink-0" />
                <span className="truncate">{acceptedMeeting.locationText}</span>
              </>
            )}
          </div>
        )}
        <div className="space-y-2 p-4">
          {messages.map((m) => {
            if (m.messageType === "Offer" && m.offer) {
              const isOwn = m.senderId === userId;
              return (
                <div
                  key={m.id}
                  className={`flex ${isOwn ? "justify-end" : "justify-start"}`}
                >
                  <OfferCard
                    offer={m.offer}
                    currentUserId={userId}
                    listingPrice={conversation.listingPrice}
                    onAccept={(offerId) =>
                      respondToOffer({ offerId, action: "Accept" })
                    }
                    onDecline={(offerId) =>
                      respondToOffer({ offerId, action: "Decline" })
                    }
                    onCounter={(offerId, amount) =>
                      respondToOffer({
                        offerId,
                        action: "Counter",
                        counterAmount: amount,
                      })
                    }
                  />
                </div>
              );
            }

            if (m.messageType === "Meeting" && m.meeting) {
              const isOwn = m.senderId === userId;
              const isAccepted = m.meeting.status === "Accepted";
              return (
                <div
                  key={m.id}
                  ref={isAccepted ? acceptedCardRefCallback : undefined}
                  className={`flex ${isOwn ? "justify-end" : "justify-start"}`}
                >
                  <MeetingCard
                    meeting={m.meeting}
                    currentUserId={userId}
                    onAccept={(meetingId) =>
                      respondToMeeting({ meetingId, action: "Accept" })
                    }
                    onDecline={(meetingId) =>
                      respondToMeeting({ meetingId, action: "Decline" })
                    }
                    onReschedule={(meetingId, data) =>
                      respondToMeeting({
                        meetingId,
                        action: "Reschedule",
                        rescheduleData: data,
                      })
                    }
                    onCancel={(meetingId) => cancelMeeting({ meetingId })}
                    onShareAvailability={(meetingId, slots) => {
                      respondToMeeting({ meetingId, action: "Decline" });
                      shareAvailability({
                        conversationId: conversation.id,
                        slots,
                      });
                    }}
                  />
                </div>
              );
            }

            if (m.messageType === "Availability" && m.availabilityCard) {
              const isOwn = m.senderId === userId;
              return (
                <div
                  key={m.id}
                  className={`flex ${isOwn ? "justify-end" : "justify-start"}`}
                >
                  <AvailabilityCardComponent
                    card={m.availabilityCard}
                    currentUserId={userId}
                    onPickSlot={(cardId, slotId, startTime, durationMinutes) =>
                      respondToAvailability({
                        availabilityCardId: cardId,
                        action: "PickSlot",
                        slotId,
                        startTime,
                        durationMinutes,
                      })
                    }
                    onShareBack={(cardId, slots) =>
                      respondToAvailability({
                        availabilityCardId: cardId,
                        action: "ShareBack",
                        shareBackSlots: slots,
                      })
                    }
                    onCancel={(cardId) =>
                      cancelAvailability({ availabilityCardId: cardId })
                    }
                  />
                </div>
              );
            }

            const isOwn = m.senderId === userId;
            return (
              <div
                key={m.id}
                className={`flex ${isOwn ? "justify-end" : "justify-start"}`}
              >
                <div
                  className={`max-w-[75%] rounded-2xl px-3 py-2 text-sm break-words ${
                    isOwn
                      ? "bg-primary text-primary-foreground rounded-br-sm"
                      : "bg-muted rounded-bl-sm"
                  }`}
                >
                  {m.content}
                </div>
              </div>
            );
          })}
          <div ref={bottomRef} />
        </div>
      </div>
      {sendError && (
        <p className="text-destructive px-3 pb-1 text-xs">{sendError}</p>
      )}
      <div className="border-border flex items-center gap-2 border-t p-3">
        <ActionBar
          currentUserId={userId}
          buyerId={conversation.buyerId}
          sellerId={conversation.sellerId}
          listingPrice={conversation.listingPrice}
          conversationId={conversation.id}
          buyerHasEngaged={conversation.buyerHasEngaged}
          hasActiveOffer={hasActiveOffer}
          hasActiveMeeting={hasActiveMeeting}
          acceptedMeeting={acceptedMeeting}
          onSendOffer={(amount) =>
            sendOffer({ conversationId: conversation.id, amount })
          }
          onProposeMeeting={(data) =>
            proposeMeeting({ conversationId: conversation.id, ...data })
          }
          onShareAvailability={(slots) =>
            shareAvailability({
              conversationId: conversation.id,
              slots,
            })
          }
          onCancelMeeting={(meetingId) => cancelMeeting({ meetingId })}
        />
        <input
          className="border-input bg-background focus:ring-ring flex-1 rounded-full border px-4 py-2 text-sm focus:ring-2 focus:outline-none"
          placeholder={`Message ${conversation.counterpartUsername}...`}
          value={input}
          onChange={(e) => {
            setInput(e.target.value);
            setSendError(null);
          }}
          onKeyDown={(e) => e.key === "Enter" && handleSend()}
        />
        <Button size="sm" onClick={handleSend} disabled={!input.trim()}>
          Send
        </Button>
      </div>
    </div>
  );
};

export default MessageThread;
