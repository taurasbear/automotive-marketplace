import { Button } from "@/components/ui/button";
import { useAppSelector } from "@/hooks/redux";
import { useSuspenseQuery } from "@tanstack/react-query";
import { useEffect, useRef, useState } from "react";
import { getMessagesOptions } from "../api/getMessagesOptions";
import { useMarkMessagesRead } from "../api/useMarkMessagesRead";
import { useChatHub } from "../api/useChatHub";
import type { ConversationSummary } from "../types/ConversationSummary";
import ActionBar from "./ActionBar";
import ListingCard from "./ListingCard";

type MessageThreadProps = {
  conversation: ConversationSummary;
  showListingCard?: boolean;
};

const MessageThread = ({
  conversation,
  showListingCard = true,
}: MessageThreadProps) => {
  const userId = useAppSelector((s) => s.auth.userId);
  const { data: messagesQuery } = useSuspenseQuery(
    getMessagesOptions({ conversationId: conversation.id }),
  );
  const messages = messagesQuery.data.messages;
  const { sendMessage } = useChatHub();
  const { mutate: markRead } = useMarkMessagesRead();
  const [input, setInput] = useState("");
  const bottomRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages.length]);

  useEffect(() => {
    markRead(conversation.id);
  }, [conversation.id, markRead]);

  const handleSend = () => {
    const trimmed = input.trim();
    if (!trimmed) return;
    sendMessage({ conversationId: conversation.id, content: trimmed });
    setInput("");
  };

  return (
    <div className="flex h-full flex-col">
      {showListingCard && (
        <ListingCard
          listingId={conversation.listingId}
          listingTitle={conversation.listingTitle}
          listingPrice={conversation.listingPrice}
          listingThumbnailUrl={conversation.listingThumbnailUrl}
        />
      )}
      <ActionBar />
      <div className="flex-1 space-y-2 overflow-y-auto p-4">
        {messages.map((m) => {
          const isOwn = m.senderId === userId;
          return (
            <div
              key={m.id}
              className={`flex ${isOwn ? "justify-end" : "justify-start"}`}
            >
              <div
                className={`max-w-[75%] rounded-2xl px-3 py-2 text-sm ${
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
      <div className="border-border flex items-center gap-2 border-t p-3">
        <input
          className="border-input bg-background focus:ring-ring flex-1 rounded-full border px-4 py-2 text-sm focus:ring-2 focus:outline-none"
          placeholder={`Message ${conversation.counterpartUsername}...`}
          value={input}
          onChange={(e) => setInput(e.target.value)}
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
