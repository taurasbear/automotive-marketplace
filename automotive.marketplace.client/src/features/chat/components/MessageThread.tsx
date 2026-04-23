import { Button } from "@/components/ui/button";
import { useAppSelector } from "@/hooks/redux";
import { useSuspenseQuery } from "@tanstack/react-query";
import { useEffect, useRef, useState } from "react";
import { getMessagesOptions } from "../api/getMessagesOptions";
import { useChatHub } from "../api/useChatHub";
import { useMarkMessagesRead } from "../api/useMarkMessagesRead";
import type { ConversationSummary } from "../types/ConversationSummary";
import ActionBar from "./ActionBar";
import ListingCard from "./ListingCard";
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
  const { sendMessage, sendOffer, respondToOffer } = useChatHub();
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
      <div className="flex-1 space-y-2 overflow-y-auto p-4">
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
          buyerHasLiked={conversation.buyerHasLiked}
          hasActiveOffer={hasActiveOffer}
          onSendOffer={(amount) =>
            sendOffer({ conversationId: conversation.id, amount })
          }
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
